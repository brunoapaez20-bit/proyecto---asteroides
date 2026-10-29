CREATE DATABASE IF NOT EXISTS videojuego
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE videojuego;

CREATE TABLE Jugadores (
    id_jugador      INT AUTO_INCREMENT PRIMARY KEY,
    nombre_usuario  VARCHAR(50)  NOT NULL,
    email           VARCHAR(100) NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    fecha_registro  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activo          TINYINT(1)   NOT NULL DEFAULT 1,  -- 1 = activo, 0 = desactivado
    fecha_baja      DATETIME     NULL,
    CONSTRAINT UQ_Jugadores_Usuario UNIQUE (nombre_usuario),
    CONSTRAINT UQ_Jugadores_Email   UNIQUE (email),
    CONSTRAINT CK_Jugadores_Activo  CHECK (activo IN (0, 1)),
    CONSTRAINT CK_Jugadores_Baja    CHECK (
        (activo = 1 AND fecha_baja IS NULL) OR
        (activo = 0 AND fecha_baja IS NOT NULL)
    )
) ENGINE=InnoDB;

CREATE TABLE Sesiones (
    id_sesion     INT AUTO_INCREMENT PRIMARY KEY,
    id_jugador    INT      NOT NULL,
    fecha_inicio  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_fin     DATETIME NULL,
    puntaje       INT      NOT NULL DEFAULT 0,
    CONSTRAINT FK_Sesiones_Jugadores FOREIGN KEY (id_jugador)
        REFERENCES Jugadores(id_jugador)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT CK_Sesiones_Fechas CHECK (fecha_fin IS NULL OR fecha_fin >= fecha_inicio),
    INDEX IX_Sesiones_Jugador (id_jugador)
) ENGINE=InnoDB;

--Procedimientos almacenados--

DELIMITER $$

CREATE PROCEDURE sp_DesactivarJugador(IN p_id_jugador INT)
BEGIN
    -- Cerrar sesiones abiertas
    UPDATE Sesiones
    SET fecha_fin = NOW()
    WHERE id_jugador = p_id_jugador AND fecha_fin IS NULL;

    UPDATE Jugadores
    SET activo = 0,
        fecha_baja = NOW()
    WHERE id_jugador = p_id_jugador AND activo = 1;

    SELECT ROW_COUNT() AS filas_afectadas;
END$$

CREATE PROCEDURE sp_ReactivarJugador(IN p_id_jugador INT)
BEGIN
    UPDATE Jugadores
    SET activo = 1,
        fecha_baja = NULL
    WHERE id_jugador = p_id_jugador AND activo = 0;

    SELECT ROW_COUNT() AS filas_afectadas;
END$$

DELIMITER ;

--Vista de los jugadores activos--

CREATE VIEW vw_JugadoresActivos AS
SELECT id_jugador, nombre_usuario, email, fecha_registro
FROM Jugadores
WHERE activo = 1;

--Trigger: bloquear sesiones de jugadores desactivados--

DELIMITER $$

CREATE TRIGGER trg_Sesiones_SoloActivos
BEFORE INSERT ON Sesiones
FOR EACH ROW
BEGIN
    DECLARE v_activo TINYINT(1);

    SELECT activo INTO v_activo
    FROM Jugadores
    WHERE id_jugador = NEW.id_jugador;

    IF v_activo = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No se puede iniciar sesión con un jugador desactivado.';
    END IF;
END$$

DELIMITER ;

--Consultas para los reportes--

-- Ranking de jugadores activos (JOIN + GROUP BY + HAVING)
SELECT j.nombre_usuario,
       COUNT(s.id_sesion)  AS total_sesiones,
       MAX(s.puntaje)      AS mejor_puntaje,
       ROUND(AVG(s.puntaje), 2) AS promedio
FROM Jugadores j
INNER JOIN Sesiones s ON s.id_jugador = j.id_jugador
WHERE j.activo = 1
GROUP BY j.id_jugador, j.nombre_usuario
HAVING COUNT(s.id_sesion) >= 3
ORDER BY mejor_puntaje DESC
LIMIT 10;

-- Histórico completo incluyendo desactivados (LEFT JOIN)
SELECT j.nombre_usuario, j.activo, COUNT(s.id_sesion) AS sesiones
FROM Jugadores j
LEFT JOIN Sesiones s ON s.id_jugador = j.id_jugador
GROUP BY j.id_jugador, j.nombre_usuario, j.activo;

-- Subquery: jugadores activos por encima del promedio global
SELECT nombre_usuario
FROM Jugadores
WHERE activo = 1
  AND id_jugador IN (
      SELECT id_jugador
      FROM Sesiones
      GROUP BY id_jugador
      HAVING AVG(puntaje) > (SELECT AVG(puntaje) FROM Sesiones)
  );
