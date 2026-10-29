using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace juego_def
{
    public class Game1 : Game
    {
        const int W = 480, H = 720;
        const float RadioNave = 11f;
        const int PuLento = 1, PuEscudo = 2, PuX2 = 3;   // Id de cada power-up

        GraphicsDeviceManager _gfx;
        SpriteBatch _sb;
        SpriteFont _font;
        Texture2D _pixel, _circulo, _triangulo;

        // ---- Datos (vienen del repositorio: archivos hoy, BD despues) ----
        IRepositorio _repo;
        TiposAsteroides[] _tipos;     // ordenado por Id (busqueda binaria)
        TipoPowerUp[] _tiposPu;       // sin orden (busqueda secuencial)
        Sesion[] _ranking;            // ordenado por tiempo (seleccion)
        string _jugador;

        // ---- Estructuras de datos ----
        ListaEnlazada<Asteroide> _rocks = new ListaEnlazada<Asteroide>();   // lista enlazada
        ListaEnlazada<PowerUp> _pups = new ListaEnlazada<PowerUp>();
        Cola<Asteroide> _colaSpawn = new Cola<Asteroide>();                 // cola: asteroides por aparecer
        Cola<EventoJuego> _eventos = new Cola<EventoJuego>();               // cola: buffer de eventos
        Pila<string> _historial = new Pila<string>();                       // pila: ultimos eventos
        GridEspacial _grid;                                                 // array 2D

        Random _rnd = new Random();

        enum Estado { Menu, Jugando, Fin }
        Estado _estado = Estado.Menu;

        Vector2 _nave;
        float _t, _puntos, _spawn, _pupT, _respT;
        int _esquivados, _cerca, _mejorAnterior, _puesto;
        bool _nuevoRecord, _hayColision;
        float[] _fx = new float[4];                  // tiempo restante de cada power-up (indice = Id)
        List<string> _ultimos = new List<string>();  // ultimos eventos (salen de la pila)
        Respaldo _respaldo;                          // partida interrumpida (si existe)

        Vector2[] _stars = new Vector2[60];
        float[] _starV = new float[60];

        KeyboardState _kb, _kbAnt;

        public Game1()
        {
            _gfx = new GraphicsDeviceManager(this);
            _gfx.PreferredBackBufferWidth = W;
            _gfx.PreferredBackBufferHeight = H;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Asteroid Dodge";
        }

        protected override void Initialize()
        {
            _repo = new RepositorioArchivos();          // <-- aca se cambia por RepositorioSql
            _jugador = Archivos.CargarJugador();

            _tipos = _repo.ObtenerTiposAsteroides();
            Algoritmos.OrdenarPorId(_tipos);            // ordenamiento por insercion
            _tiposPu = _repo.ObtenerTiposPowerUp();

            _grid = new GridEspacial(W, H);
            CargarRanking();
            _respaldo = Archivos.CargarRespaldo();      // recuperacion ante cierre inesperado

            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new Vector2(Rango(0, W), Rango(0, H));
                _starV[i] = Rango(10, 50);
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Fuente");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _circulo = CrearCirculo(64);
            _triangulo = CrearTriangulo(32, 40);
        }

        protected override void UnloadContent()
        {
            // Las texturas creadas por codigo no las libera el Content: las liberamos nosotros
            if (_pixel != null) _pixel.Dispose();
            if (_circulo != null) _circulo.Dispose();
            if (_triangulo != null) _triangulo.Dispose();
            base.UnloadContent();
        }

        // ---------- Texturas generadas por codigo ----------
        Texture2D CrearCirculo(int d)
        {
            Texture2D tx = new Texture2D(GraphicsDevice, d, d);
            Color[] dat = new Color[d * d];
            float r = d / 2f;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    if (dx * dx + dy * dy <= r * r) dat[y * d + x] = Color.White;
                }
            tx.SetData(dat);
            return tx;
        }

        Texture2D CrearTriangulo(int w, int h)
        {
            Texture2D tx = new Texture2D(GraphicsDevice, w, h);
            Color[] dat = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float mitad = (y + 1) / (float)h * (w / 2f);
                for (int x = 0; x < w; x++)
                    if (Math.Abs(x + 0.5f - w / 2f) <= mitad) dat[y * w + x] = Color.White;
            }
            tx.SetData(dat);
            return tx;
        }

        // ---------- Utilidades ----------
        float Rango(float a, float b) { return a + (float)_rnd.NextDouble() * (b - a); }

        static float Radio(int id) { return id == 1 ? 12f : id == 2 ? 20f : 30f; }

        static Color ColorTipo(int id)
        {
            return id == 1 ? new Color(184, 196, 217)
                 : id == 2 ? new Color(196, 154, 108)
                 : new Color(154, 127, 184);
        }

        // Busqueda binaria sobre los tipos de asteroide
        TiposAsteroides Tipo(int id)
        {
            int pos = Algoritmos.BuscarPorId(_tipos, id);
            return pos >= 0 ? _tipos[pos] : null;
        }

        // La fuente por defecto solo dibuja ASCII: lo demas se reemplaza por '?'
        static string Seguro(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s) sb.Append(c >= 32 && c <= 126 ? c : '?');
            return sb.ToString();
        }

        static string Cortar(string s, int max) { return s.Length > max ? s.Substring(0, max) : s; }

        void Texto(string s, float x, float y, Color c, float esc = 1.3f, bool centro = false)
        {
            s = Seguro(s);
            if (centro) x -= _font.MeasureString(s).X * esc / 2f;
            _sb.DrawString(_font, s, new Vector2(x, y), c, 0f, Vector2.Zero, esc, SpriteEffects.None, 0f);
        }

        bool Nueva(Keys k) { return _kb.IsKeyDown(k) && !_kbAnt.IsKeyDown(k); }

        void CargarRanking()
        {
            _ranking = _repo.ObtenerSesiones();
            Algoritmos.OrdenarRanking(_ranking);        // ordenamiento por seleccion
        }

        // ---------- Logica ----------
        void IniciarPartida(Respaldo r)
        {
            _rocks.Vaciar(); _pups.Vaciar(); _colaSpawn.Vaciar();
            _eventos.Vaciar(); _historial.Vaciar(); _grid.Limpiar();

            _nave = new Vector2(W / 2f, H - 90);
            _t = 0; _puntos = 0; _esquivados = 0; _cerca = 0;
            _spawn = 0.8f; _pupT = 6f; _respT = 0; _hayColision = false;
            Array.Clear(_fx, 0, _fx.Length);

            if (r != null)   // continuar partida interrumpida
            {
                _t = r.T; _puntos = r.Puntos; _esquivados = r.Esquivados; _cerca = r.Cerca;
                _nave = new Vector2(r.NaveX, r.NaveY);
                Array.Copy(r.Fx, _fx, 4);
            }
            else
            {
                Archivos.BorrarRespaldo();
            }
            _respaldo = null;

            LlenarColaSpawn(8);
            _estado = Estado.Jugando;
        }

        // Genera una oleada de asteroides y los encola (FIFO)
        void LlenarColaSpawn(int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double p = _rnd.NextDouble();
                int id = p < 0.5 ? 1 : p < 0.82 ? 2 : 3;
                float r = Radio(id);
                _colaSpawn.Encolar(new Asteroide
                {
                    Pos = new Vector2(Rango(r, W - r), -r),
                    VelAng = Rango(-2, 2),
                    TypeId = id,
                    Apunta = _rnd.NextDouble() < 0.3
                });
            }
        }

        // Validacion recursiva: el asteroide solo entra si el jugador sigue teniendo una ruta libre
        bool RutaDisponible(Asteroide a)
        {
            int fa, ca, fn, cn;
            _grid.ObtenerCelda(a.Pos, out fa, out ca);
            _grid.ObtenerCelda(_nave, out fn, out cn);
            bool[,] ocupado = _grid.Ocupacion();
            ocupado[fa, ca] = true;
            return Algoritmos.RutaLibre(ocupado, fn, cn);
        }

        void Evento(TipoEvento tipo, int tipoAst, int puntos, float dist, string desc)
        {
            _eventos.Encolar(new EventoJuego
            {
                Tipo = tipo,
                Tiempo = _t,
                AsteroideTipo = tipoAst,
                Puntos = puntos,
                Distancia = dist,
                Descripcion = _t.ToString("0") + "s " + desc
            });
        }

        // Atiende los eventos en el orden en que ocurrieron (FIFO)
        void ProcesarEventos()
        {
            while (!_eventos.EstaVacia)
            {
                EventoJuego e = _eventos.Desencolar();
                switch (e.Tipo)
                {
                    case TipoEvento.Evasion: _esquivados++; _puntos += e.Puntos; break;
                    case TipoEvento.Escudo: _puntos += e.Puntos; break;
                    case TipoEvento.Cercania: _cerca++; break;
                    case TipoEvento.Colision: _hayColision = true; break;
                }
                _historial.Apilar(e.Descripcion);   // pila: el ultimo evento queda arriba
                _repo.RegistrarEvento(e);
            }
            if (_hayColision) FinDelJuego();
        }

        void FinDelJuego()
        {
            _estado = Estado.Fin;

            Sesion s = new Sesion
            {
                Jugador = _jugador,
                TiempoSupervivencia = (int)_t,
                Esquivados = _esquivados,
                Cerca = _cerca,
                Puntos = (int)_puntos,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            // Busqueda secuencial: mejor marca anterior del jugador (el ranking esta ordenado)
            int idx = Algoritmos.BuscarJugador(_ranking, _jugador);
            _mejorAnterior = idx >= 0 ? _ranking[idx].TiempoSupervivencia : 0;
            _nuevoRecord = s.TiempoSupervivencia > _mejorAnterior;

            _repo.GuardarSesion(s);
            Archivos.BorrarRespaldo();
            _respaldo = null;
            CargarRanking();

            _puesto = _ranking.Length;
            for (int i = 0; i < _ranking.Length; i++)
                if (_ranking[i].TiempoSupervivencia <= s.TiempoSupervivencia) { _puesto = i + 1; break; }

            // Sacamos de la pila los ultimos 4 eventos (el mas reciente primero)
            _ultimos.Clear();
            for (int i = 0; i < 4 && !_historial.EstaVacia; i++) _ultimos.Add(_historial.Desapilar());
            _historial.Vaciar();
        }

        protected override void Update(GameTime gt)
        {
            float dt = Math.Min(0.05f, (float)gt.ElapsedGameTime.TotalSeconds);
            _kbAnt = _kb;
            _kb = Keyboard.GetState();
            if (_kb.IsKeyDown(Keys.Escape)) Exit();

            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].Y += _starV[i] * dt * (_estado == Estado.Jugando ? 1f : 0.4f);
                if (_stars[i].Y > H) { _stars[i].Y = 0; _stars[i].X = Rango(0, W); }
            }

            if (_estado == Estado.Jugando) Jugar(dt);
            else if (_estado == Estado.Menu && Nueva(Keys.C) && _respaldo != null) IniciarPartida(_respaldo);
            else if (Nueva(Keys.Enter)) IniciarPartida(null);

            base.Update(gt);
        }

        void Jugar(float dt)
        {
            _t += dt;
            int nivel = (int)(_t / 10f);
            // Dificultad progresiva: factor recursivo por nivel + avance suave dentro del nivel
            float diff = Math.Min(3.5f, Algoritmos.FactorDificultad(nivel) + 0.025f * (_t - nivel * 10f));
            float lento = _fx[PuLento] > 0 ? 0.45f : 1f;
            int mult = _fx[PuX2] > 0 ? 2 : 1;
            _puntos += dt * 5 * mult;
            for (int k = 1; k <= 3; k++) _fx[k] = Math.Max(0, _fx[k] - dt);

            // ---- Movimiento de la nave ----
            Vector2 dir = Vector2.Zero;
            if (_kb.IsKeyDown(Keys.Left) || _kb.IsKeyDown(Keys.A)) dir.X -= 1;
            if (_kb.IsKeyDown(Keys.Right) || _kb.IsKeyDown(Keys.D)) dir.X += 1;
            if (_kb.IsKeyDown(Keys.Up) || _kb.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (_kb.IsKeyDown(Keys.Down) || _kb.IsKeyDown(Keys.S)) dir.Y += 1;
            if (dir != Vector2.Zero) dir.Normalize();   // sin esto la diagonal va ~41% mas rapido
            _nave += dir * 300f * dt;
            _nave.X = MathHelper.Clamp(_nave.X, 20, W - 20);
            _nave.Y = MathHelper.Clamp(_nave.Y, 30, H - 25);

            // ---- Aparicion de asteroides (sale de la cola) ----
            _spawn -= dt;
            if (_spawn <= 0)
            {
                _spawn = Math.Max(0.22f, 0.9f - _t * 0.012f) * Rango(0.7f, 1.3f);
                if (_colaSpawn.EstaVacia) LlenarColaSpawn(8);
                Asteroide nuevo = _colaSpawn.Desencolar();
                nuevo.VelX = nuevo.Apunta ? (_nave.X - nuevo.Pos.X) / 4f : Rango(-30, 30);
                if (RutaDisponible(nuevo)) _rocks.Agregar(nuevo);
            }

            // ---- Aparicion de power-ups ----
            _pupT -= dt;
            if (_pupT <= 0)
            {
                _pups.Agregar(new PowerUp { Pos = new Vector2(Rango(30, W - 30), -20), Tipo = _rnd.Next(1, 4) });
                _pupT = Rango(7, 11);
            }

            // ---- Mover asteroides (los fuera de pantalla son evasiones exitosas) ----
            foreach (Asteroide o in _rocks)
            {
                TiposAsteroides tp = Tipo(o.TypeId);            // busqueda binaria
                if (tp == null) { o.Activo = false; continue; }
                float r = Radio(o.TypeId);

                o.Pos.Y += tp.VelocidadBase * 60f * diff * lento * dt;
                o.Pos.X += o.VelX * lento * dt;
                o.Angulo += o.VelAng * dt;

                if (o.Pos.Y > H + r || o.Pos.X < -60 || o.Pos.X > W + 60)
                {
                    o.Activo = false;
                    // Entro a la zona de peligro y salio sin chocar: casi choque
                    if (o.EnZonaCerca)
                        Evento(TipoEvento.Cercania, o.TypeId, 0, o.DistMin, "Paso cerca de un asteroide");
                    Evento(TipoEvento.Evasion, o.TypeId, tp.PuntosEvasion * mult, 0f, "Esquivaste " + tp.Tamano);
                }
            }

            // ---- Reconstruir la grilla 2D y revisar solo los asteroides cercanos ----
            _grid.Limpiar();
            foreach (Asteroide o in _rocks)
                if (o.Activo) _grid.Insertar(o);

            foreach (Asteroide o in _grid.Cercanos(_nave))
            {
                TiposAsteroides tp = Tipo(o.TypeId);
                if (tp == null) continue;
                float r = Radio(o.TypeId);
                float d = Vector2.Distance(o.Pos, _nave);

                if (d < r * 0.85f + RadioNave)                   // colision
                {
                    if (_fx[PuEscudo] > 0)
                    {
                        o.Activo = false;
                        Evento(TipoEvento.Escudo, o.TypeId, tp.PuntosEvasion * mult, d, "Escudo destruyo " + tp.Tamano);
                    }
                    else
                    {
                        Evento(TipoEvento.Colision, o.TypeId, 0, d, "Choque con asteroide " + tp.Tamano);
                        break;                                   // un solo choque por frame
                    }
                }
                else if (d < r + RadioNave + 25)                 // zona de peligro (proximidad)
                {
                    o.EnZonaCerca = true;                        // se cuenta recien al esquivarlo
                    o.DistMin = Math.Min(o.DistMin, d);
                }
            }

            // ---- Power-ups ----
            foreach (PowerUp p in _pups)
            {
                p.Pos.Y += 90f * dt;
                if (Vector2.Distance(p.Pos, _nave) < RadioNave + 14)
                {
                    TipoPowerUp tpu = Algoritmos.BuscarPowerUp(_tiposPu, p.Tipo);   // busqueda secuencial
                    if (tpu != null)
                    {
                        _fx[p.Tipo] = tpu.Duracion;
                        Evento(TipoEvento.PowerUp, 0, 0, 0f, "Tomaste " + tpu.Nombre);
                    }
                    p.Activo = false;
                }
                else if (p.Pos.Y > H + 20) p.Activo = false;
            }

            // ---- Limpiar de las listas enlazadas lo que ya no esta activo ----
            _rocks.RemoverDonde(ast => !ast.Activo);
            _pups.RemoverDonde(pw => !pw.Activo);

            ProcesarEventos();
            if (_estado != Estado.Jugando) return;

            // ---- Respaldo binario de la sesion activa cada 3 segundos ----
            _respT += dt;
            if (_respT >= 3f)
            {
                _respT = 0;
                Respaldo rs = new Respaldo
                {
                    T = _t,
                    Puntos = _puntos,
                    NaveX = _nave.X,
                    NaveY = _nave.Y,
                    Esquivados = _esquivados,
                    Cerca = _cerca
                };
                Array.Copy(_fx, rs.Fx, 4);
                Archivos.GuardarRespaldo(rs);
            }
        }

        // ---------- Dibujo ----------
        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(new Color(5, 11, 24));
            _sb.Begin();

            for (int i = 0; i < _stars.Length; i++)
                _sb.Draw(_pixel, new Rectangle((int)_stars[i].X, (int)_stars[i].Y, 2, 2), Color.White * 0.7f);

            if (_fx[PuLento] > 0)
                _sb.Draw(_pixel, new Rectangle(0, 0, W, H), new Color(92, 225, 255) * 0.07f);

            foreach (Asteroide o in _rocks)
            {
                float r = Radio(o.TypeId);
                Vector2 centro = new Vector2(32, 32);
                _sb.Draw(_circulo, o.Pos, null, ColorTipo(o.TypeId), o.Angulo, centro,
                         r * 2f / 64f, SpriteEffects.None, 0f);
                Vector2 crater = Vector2.Transform(new Vector2(r * 0.3f, -r * 0.2f),
                                                   Matrix.CreateRotationZ(o.Angulo));
                _sb.Draw(_circulo, o.Pos + crater, null, Color.Black * 0.3f, 0f, centro,
                         r * 0.44f / 64f, SpriteEffects.None, 0f);
            }

            foreach (PowerUp p in _pups)
            {
                TipoPowerUp tpu = Algoritmos.BuscarPowerUp(_tiposPu, p.Tipo);
                if (tpu == null) continue;
                _sb.Draw(_circulo, p.Pos, null, tpu.Color * 0.35f, 0f, new Vector2(32, 32),
                         28f / 64f, SpriteEffects.None, 0f);
                Texto(tpu.Letra, p.Pos.X, p.Pos.Y - 9, tpu.Color, 1.0f, true);
            }

            if (_estado != Estado.Menu) DibujarNave();

            if (_estado == Estado.Jugando) DibujarHud();
            else if (_estado == Estado.Menu) DibujarMenu();
            else DibujarFin();

            _sb.End();
            base.Draw(gt);
        }

        void DibujarNave()
        {
            int x = (int)_nave.X, y = (int)_nave.Y;
            if (_estado == Estado.Jugando)   // llama
            {
                int largo = 8 + _rnd.Next(10);
                _sb.Draw(_pixel, new Rectangle(x - 4, y + 12, 8, largo), new Color(255, 159, 67));
            }
            Color rosa = new Color(255, 107, 139);
            _sb.Draw(_pixel, new Rectangle(x - 18, y + 2, 8, 14), rosa);   // alas
            _sb.Draw(_pixel, new Rectangle(x + 10, y + 2, 8, 14), rosa);
            _sb.Draw(_triangulo, _nave, null, new Color(216, 232, 255), 0f,
                     new Vector2(16, 20), 1f, SpriteEffects.None, 0f);       // cuerpo
            _sb.Draw(_circulo, _nave + new Vector2(0, -2), null, new Color(92, 225, 255), 0f,
                     new Vector2(32, 32), 12f / 64f, SpriteEffects.None, 0f); // cabina
            if (_fx[PuEscudo] > 0)                                            // escudo
                _sb.Draw(_circulo, _nave, null, new Color(255, 216, 77) * 0.3f, 0f,
                         new Vector2(32, 32), 50f / 64f, SpriteEffects.None, 0f);
        }

        void DibujarHud()
        {
            Texto("Tiempo " + _t.ToString("0.0") + " s", 12, 8, Color.White);
            Texto("Esquivados " + _esquivados, 12, 32, Color.White);
            Texto("Puntos " + (int)_puntos, W - 150, 8, Color.White);
            float y = 32;
            for (int id = 1; id <= 3; id++)
                if (_fx[id] > 0)
                {
                    TipoPowerUp tpu = Algoritmos.BuscarPowerUp(_tiposPu, id);
                    if (tpu == null) continue;
                    Texto(tpu.Nombre + " " + _fx[id].ToString("0.0"), W - 150, y, tpu.Color, 1.0f);
                    y += 22;
                }
        }

        void Panel(int alto)
        {
            _sb.Draw(_pixel, new Rectangle(40, (H - alto) / 2, W - 80, alto), new Color(5, 11, 24) * 0.9f);
        }

        void DibujarRanking(float y, int max)
        {
            Texto("TOP " + max + " - MEJORES TIEMPOS", W / 2f, y, new Color(125, 255, 154), 1.1f, true);
            if (_ranking.Length == 0)
                Texto("Todavia no hay partidas", W / 2f, y + 30, new Color(207, 227, 255), 1.0f, true);
            for (int i = 0; i < _ranking.Length && i < max; i++)
            {
                Sesion s = _ranking[i];
                Texto((i + 1) + ". " + Cortar(s.Jugador, 12) + "  " + s.TiempoSupervivencia + " s  " + s.Puntos + " pts",
                      W / 2f, y + 30 + i * 24, new Color(207, 227, 255), 1.0f, true);
            }
        }

        void DibujarMenu()
        {
            Panel(540);
            Color claro = new Color(207, 227, 255), amarillo = new Color(255, 216, 77);
            Texto("ASTEROID DODGE", W / 2f, 120, Color.White, 2.0f, true);
            Texto("Esquiva los asteroides el mayor tiempo posible", W / 2f, 185, claro, 0.95f, true);
            Texto("Flechas o WASD para moverte", W / 2f, 210, claro, 0.95f, true);
            Texto("L = lento   E = escudo   x2 = puntos dobles", W / 2f, 235, claro, 0.95f, true);
            Texto("Presiona ENTER para jugar", W / 2f, 285, amarillo, 1.3f, true);
            if (_respaldo != null)
                Texto("Partida interrumpida: C para continuar", W / 2f, 325, new Color(255, 107, 139), 1.0f, true);
            DibujarRanking(375, 5);
            Texto("Jugador: " + Cortar(_jugador, 20), W / 2f, 590, claro, 1.0f, true);
        }

        void DibujarFin()
        {
            Panel(540);
            Color claro = new Color(207, 227, 255), amarillo = new Color(255, 216, 77);
            Texto("CHOCASTE!", W / 2f, 110, new Color(255, 107, 139), 2.0f, true);
            Texto("Tiempo: " + _t.ToString("0.0") + " s", W / 2f, 170, Color.White, 1.2f, true);
            Texto("Esquivados: " + _esquivados, W / 2f, 198, Color.White, 1.2f, true);
            Texto("Pasaron cerca: " + _cerca, W / 2f, 226, Color.White, 1.2f, true);
            Texto("Puntos: " + (int)_puntos, W / 2f, 254, Color.White, 1.2f, true);

            Texto(_nuevoRecord ? "NUEVO RECORD PERSONAL!" : "Tu mejor marca: " + _mejorAnterior + " s",
                  W / 2f, 292, new Color(125, 255, 154), 1.2f, true);
            Texto("Puesto " + _puesto + " de " + _ranking.Length + " en el ranking", W / 2f, 320, claro, 1.0f, true);

            Texto("Ultimos eventos:", W / 2f, 358, amarillo, 1.0f, true);
            for (int i = 0; i < _ultimos.Count; i++)
                Texto(_ultimos[i], W / 2f, 382 + i * 20, claro, 0.9f, true);

            DibujarRanking(475, 3);
            Texto("ENTER para reintentar", W / 2f, 585, amarillo, 1.2f, true);
        }
    }
}