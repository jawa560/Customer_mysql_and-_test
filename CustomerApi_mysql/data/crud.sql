-- phpMyAdmin SQL Dump
-- version 4.9.1
-- https://www.phpmyadmin.net/
--
-- 主機： localhost
-- 產生時間： 
-- 伺服器版本： 8.0.17
-- PHP 版本： 7.3.10

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- 資料庫： `crud`
--

-- --------------------------------------------------------

--
-- 資料表結構 `customers`
--

CREATE TABLE `customers` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Gender` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Birthday` date NOT NULL,
  `Address` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Phone` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Note1` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Note2` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- 傾印資料表的資料 `customers`
--

INSERT INTO `customers` (`Id`, `Name`, `Gender`, `Birthday`, `Address`, `Phone`, `Note1`, `Note2`) VALUES
(1, '王曉明1', '男', '1990-01-01', '台南市永華路一段1號', '123-456-7890', '註解 1', '註解 2'),
(2, '王曉明2', '男', '1990-01-01', '台南市永華路一段1號', '123-456-7890', '註解 1', '註解 2'),
(3, '王曉明3', '男', '1990-01-01', '台南市永華路一段1號', '123-456-7890', '註解 1', '註解 2'),
(4, '王曉明4', '男', '1955-01-01', '台南市永華路一段1號', '123-456-7890', '註解 1', '註解 2');

-- --------------------------------------------------------

--
-- 資料表結構 `items`
--

CREATE TABLE `items` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- 資料表結構 `tokens`
--

CREATE TABLE `tokens` (
  `Id` int(11) NOT NULL,
  `Username` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TokenValue` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ExpiryDate` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- 傾印資料表的資料 `tokens`
--

INSERT INTO `tokens` (`Id`, `Username`, `TokenValue`, `ExpiryDate`) VALUES
(3, 'james', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqYW1lcyIsImp0aSI6IjU4ODBjZjNmLWM5OTUtNGRiNC04Yjk5LWI5NTU4MWZlOTljMyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3Mjk2OTg0MzcsImlzcyI6InNlbGZfaXNzdWVyIiwiYXVkIjoic2VsZl9hdWRpZW5jZSJ9.wzKe3JSFpS6K8EE5J33OGqoEYhFuHqhIFvzLqGvDRFk', '2024-10-24 00:17:18'),
(4, 'god', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJnb2QiLCJqdGkiOiI5NjI2MjAzZS0zZWJmLTQ2OWUtOTVkOC0wNmEyMWVhOTY4YzgiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTcyOTcwMDEwMywiaXNzIjoic2VsZl9pc3N1ZXIiLCJhdWQiOiJzZWxmX2F1ZGllbmNlIn0.RhmG83_HRcdTlQLfytnb-Jg-9qpAKv5NDGiv_QBobf4', '2024-10-24 00:45:04');

-- --------------------------------------------------------

--
-- 資料表結構 `users`
--

CREATE TABLE `users` (
  `Id` int(11) NOT NULL,
  `Username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Password` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Role` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- 傾印資料表的資料 `users`
--

INSERT INTO `users` (`Id`, `Username`, `Password`, `Role`) VALUES
(1, 'james', '1234', 'User'),
(2, 'god', '123456', 'Admin');

--
-- 已傾印資料表的索引
--

--
-- 資料表索引 `customers`
--
ALTER TABLE `customers`
  ADD PRIMARY KEY (`Id`);

--
-- 資料表索引 `items`
--
ALTER TABLE `items`
  ADD PRIMARY KEY (`Id`);

--
-- 資料表索引 `tokens`
--
ALTER TABLE `tokens`
  ADD UNIQUE KEY `Id` (`Id`) USING BTREE;

--
-- 資料表索引 `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`);

--
-- 在傾印的資料表使用自動遞增(AUTO_INCREMENT)
--

--
-- 使用資料表自動遞增(AUTO_INCREMENT) `tokens`
--
ALTER TABLE `tokens`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
