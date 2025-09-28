-- MySQL Initialization Script for NetCorePal Template
-- This script creates the necessary database and user for development

-- Create development database if it doesn't exist
CREATE DATABASE IF NOT EXISTS `abctemplate` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create a development user (optional - you can use root for development)
-- CREATE USER IF NOT EXISTS 'devuser'@'%' IDENTIFIED BY 'devpass123';
-- GRANT ALL PRIVILEGES ON `abctemplate`.* TO 'devuser'@'%';

-- Ensure root can connect from any host (for development only)
-- ALTER USER 'root'@'%' IDENTIFIED BY '123456';
-- GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;

FLUSH PRIVILEGES;

-- Display completion message
SELECT 'MySQL initialization completed successfully' AS message;