CREATE TABLE Jugadores (
    id_jugador      INT AUTO_INCREMENT(1,1) PRIMARY KEY,
    nombre_usuario  VARCHAR(50)  NOT NULL,
    email           VARCHAR(100) NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    fecha_registro  DATETIME     NOT NULL DEFAULT GETDATE(),
    activo          BIT          NOT NULL DEFAULT 1,   -- 1 = activo, 0 = desactivado
    fecha_baja      DATETIME     NULL,                 -- cuándo se desactivó
    CONSTRAINT UQ_Jugadores_Usuario UNIQUE (nombre_usuario),
    CONSTRAINT UQ_Jugadores_Email   UNIQUE (email),
    -- Integridad de dominio: si está desactivado, debe tener fecha de baja
    CONSTRAINT CK_Jugadores_Baja CHECK (
        (activo = 1 AND fecha_baja IS NULL) OR
        (activo = 0 AND fecha_baja IS NOT NULL)
    )
);

CREATE TABLE Sesiones (
    id_sesion     INT AUTO_INCREMENT(1,1) PRIMARY KEY,
    id_jugador    INT      NOT NULL,
    fecha_inicio  DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_fin     DATETIME NULL,
    puntaje       INT      NOT NULL DEFAULT 0,
    CONSTRAINT FK_Sesiones_Jugadores FOREIGN KEY (id_jugador)
        REFERENCES Jugadores(id_jugador)
        ON DELETE NO ACTION,   -- evita borrar jugadores con sesiones
    CONSTRAINT CK_Sesiones_Fechas CHECK (fecha_fin IS NULL OR fecha_fin >= fecha_inicio)
);

CREATE INDEX IX_Sesiones_Jugador ON Sesiones(id_jugador);
