# proyecto---asteroides
CREATE TABLE Player (
	id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100)
);

CREATE TABLE Sessions(
	id INT PRIMARY KEY AUTO_INCREMENT,
    id_Player INT,
    inicio DATETIME,
    fin DATETIME,
    tiempo_superviviencia VARCHAR,
    dificultad_inicial INT,
    FOREIGN KEY (id_Player) REFERENCES Player(id)
);
