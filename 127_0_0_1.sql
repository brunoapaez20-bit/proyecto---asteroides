-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 20-10-2026 a las 00:11:37
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `juego_asteroides`
--
DROP DATABASE IF EXISTS `juego_asteroides`;
CREATE DATABASE IF NOT EXISTS `juego_asteroides` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `juego_asteroides`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `asteroidesenpantalla`
--

DROP TABLE IF EXISTS `asteroidesenpantalla`;
CREATE TABLE `asteroidesenpantalla` (
  `id` int(11) NOT NULL,
  `partida_id` int(11) NOT NULL,
  `tipo_asteroide_id` int(11) NOT NULL,
  `x` decimal(6,2) NOT NULL,
  `y` decimal(6,2) NOT NULL,
  `velocidad_actual` decimal(5,2) NOT NULL,
  `spawn_time` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `choques`
--

DROP TABLE IF EXISTS `choques`;
CREATE TABLE `choques` (
  `id` int(11) NOT NULL,
  `partida_id` int(11) NOT NULL,
  `asteroide_pantalla_id` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp(),
  `coordenada_impacto` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `esquives`
--

DROP TABLE IF EXISTS `esquives`;
CREATE TABLE `esquives` (
  `id` int(11) NOT NULL,
  `partida_id` int(11) NOT NULL,
  `asteroide_pantalla_id` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp(),
  `distancia_minima` decimal(5,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `jugadores`
--

DROP TABLE IF EXISTS `jugadores`;
CREATE TABLE `jugadores` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `fecha_registro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `mejoras`
--

DROP TABLE IF EXISTS `mejoras`;
CREATE TABLE `mejoras` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `efecto` varchar(255) NOT NULL,
  `duracion_base` int(11) NOT NULL,
  `probabilidad_spawn` decimal(4,3) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `mejoras`
--

INSERT INTO `mejoras` (`id`, `nombre`, `efecto`, `duracion_base`, `probabilidad_spawn`) VALUES
(1, 'Escudo', 'Protege de un choque contra cualquier asteroide', 5, 0.050),
(2, 'Cámara Lenta', 'Reduce la velocidad de los asteroides a la mitad', 8, 0.080),
(3, 'Imán de Puntos', 'Duplica los puntos por cada esquive exitoso', 10, 0.040),
(4, 'Bomba Espacial', 'Destruye todos los asteroides visibles en pantalla', 0, 0.020);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `mejorasusadas`
--

DROP TABLE IF EXISTS `mejorasusadas`;
CREATE TABLE `mejorasusadas` (
  `id` int(11) NOT NULL,
  `partida_id` int(11) NOT NULL,
  `mejora_id` int(11) NOT NULL,
  `timestamp_aplicacion` timestamp NOT NULL DEFAULT current_timestamp(),
  `timestamp_fin` timestamp NULL DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `partidas`
--

DROP TABLE IF EXISTS `partidas`;
CREATE TABLE `partidas` (
  `id` int(11) NOT NULL,
  `jugador_id` int(11) NOT NULL,
  `inicio` timestamp NOT NULL DEFAULT current_timestamp(),
  `fin` timestamp NULL DEFAULT NULL,
  `tiempo_supervivencia` int(11) NOT NULL DEFAULT 0,
  `dificultad_inicial` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tiposasteroides`
--

DROP TABLE IF EXISTS `tiposasteroides`;
CREATE TABLE `tiposasteroides` (
  `id` int(11) NOT NULL,
  `tamaño` varchar(20) NOT NULL,
  `velocidad_base` decimal(5,2) NOT NULL,
  `sprite` varchar(100) NOT NULL,
  `puntos_evasion` int(11) NOT NULL DEFAULT 10
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `tiposasteroides`
--

INSERT INTO `tiposasteroides` (`id`, `tamaño`, `velocidad_base`, `sprite`, `puntos_evasion`) VALUES
(1, 'Pequeño', 8.50, 'sprites/meteor_small.png', 15),
(2, 'Mediano', 5.00, 'sprites/meteor_medium.png', 10),
(3, 'Grande', 2.50, 'sprites/meteor_big.png', 5),
(4, 'Veloz', 15.00, 'sprites/meteor_fast.png', 30);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `asteroidesenpantalla`
--
ALTER TABLE `asteroidesenpantalla`
  ADD PRIMARY KEY (`id`),
  ADD KEY `partida_id` (`partida_id`),
  ADD KEY `tipo_asteroide_id` (`tipo_asteroide_id`);

--
-- Indices de la tabla `choques`
--
ALTER TABLE `choques`
  ADD PRIMARY KEY (`id`),
  ADD KEY `partida_id` (`partida_id`),
  ADD KEY `asteroide_pantalla_id` (`asteroide_pantalla_id`);

--
-- Indices de la tabla `esquives`
--
ALTER TABLE `esquives`
  ADD PRIMARY KEY (`id`),
  ADD KEY `partida_id` (`partida_id`),
  ADD KEY `asteroide_pantalla_id` (`asteroide_pantalla_id`);

--
-- Indices de la tabla `jugadores`
--
ALTER TABLE `jugadores`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`),
  ADD UNIQUE KEY `email` (`email`);

--
-- Indices de la tabla `mejoras`
--
ALTER TABLE `mejoras`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `mejorasusadas`
--
ALTER TABLE `mejorasusadas`
  ADD PRIMARY KEY (`id`),
  ADD KEY `partida_id` (`partida_id`),
  ADD KEY `mejora_id` (`mejora_id`);

--
-- Indices de la tabla `partidas`
--
ALTER TABLE `partidas`
  ADD PRIMARY KEY (`id`),
  ADD KEY `jugador_id` (`jugador_id`);

--
-- Indices de la tabla `tiposasteroides`
--
ALTER TABLE `tiposasteroides`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `asteroidesenpantalla`
--
ALTER TABLE `asteroidesenpantalla`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `choques`
--
ALTER TABLE `choques`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `esquives`
--
ALTER TABLE `esquives`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `jugadores`
--
ALTER TABLE `jugadores`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `mejoras`
--
ALTER TABLE `mejoras`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `mejorasusadas`
--
ALTER TABLE `mejorasusadas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `partidas`
--
ALTER TABLE `partidas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `tiposasteroides`
--
ALTER TABLE `tiposasteroides`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `asteroidesenpantalla`
--
ALTER TABLE `asteroidesenpantalla`
  ADD CONSTRAINT `asteroidesenpantalla_ibfk_1` FOREIGN KEY (`partida_id`) REFERENCES `partidas` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `asteroidesenpantalla_ibfk_2` FOREIGN KEY (`tipo_asteroide_id`) REFERENCES `tiposasteroides` (`id`);

--
-- Filtros para la tabla `choques`
--
ALTER TABLE `choques`
  ADD CONSTRAINT `choques_ibfk_1` FOREIGN KEY (`partida_id`) REFERENCES `partidas` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `choques_ibfk_2` FOREIGN KEY (`asteroide_pantalla_id`) REFERENCES `asteroidesenpantalla` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `esquives`
--
ALTER TABLE `esquives`
  ADD CONSTRAINT `esquives_ibfk_1` FOREIGN KEY (`partida_id`) REFERENCES `partidas` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `esquives_ibfk_2` FOREIGN KEY (`asteroide_pantalla_id`) REFERENCES `asteroidesenpantalla` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `mejorasusadas`
--
ALTER TABLE `mejorasusadas`
  ADD CONSTRAINT `mejorasusadas_ibfk_1` FOREIGN KEY (`partida_id`) REFERENCES `partidas` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `mejorasusadas_ibfk_2` FOREIGN KEY (`mejora_id`) REFERENCES `mejoras` (`id`);

--
-- Filtros para la tabla `partidas`
--
ALTER TABLE `partidas`
  ADD CONSTRAINT `partidas_ibfk_1` FOREIGN KEY (`jugador_id`) REFERENCES `jugadores` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
