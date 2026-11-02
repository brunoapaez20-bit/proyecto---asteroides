CREATE TABLE dificultades (
  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(20) NOT NULL UNIQUE
);
INSERT INTO dificultades (nombre)
  SELECT DISTINCT dificultad_inicial FROM partidas;

ALTER TABLE partidas ADD dificultad_id INT NULL;
UPDATE partidas p JOIN dificultades d
  ON d.nombre = p.dificultad_inicial
  SET p.dificultad_id = d.id;
ALTER TABLE partidas
  MODIFY dificultad_id INT NOT NULL,
  ADD FOREIGN KEY (dificultad_id) REFERENCES dificultades(id);
ALTER TABLE partidas DROP COLUMN dificultad_inicial;
ALTER TABLE partidas DROP COLUMN tiempo_supervivencia;

ALTER TABLE choques ADD x DECIMAL(6,2) NULL, ADD y DECIMAL(6,2) NULL;
ALTER TABLE choques DROP COLUMN coordenada_impacto;

ALTER TABLE choques DROP FOREIGN KEY choques_ibfk_1;
ALTER TABLE choques DROP COLUMN partida_id;
ALTER TABLE esquives DROP FOREIGN KEY esquives_ibfk_1;
ALTER TABLE esquives DROP COLUMN partida_id