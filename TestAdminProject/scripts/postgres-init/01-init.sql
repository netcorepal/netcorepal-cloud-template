-- PostgreSQL Initialization Script for NetCorePal Template
-- This script creates the necessary database and user for development

-- Create development database if it doesn't exist
SELECT 'CREATE DATABASE abctemplate'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'abctemplate')\gexec

-- Create a development user (optional - you can use postgres for development)
-- DO
-- $do$
-- BEGIN
--    IF NOT EXISTS (
--       SELECT FROM pg_catalog.pg_roles
--       WHERE  rolname = 'devuser') THEN
--       CREATE ROLE devuser LOGIN PASSWORD 'devpass123';
--    END IF;
-- END
-- $do$;

-- Grant privileges to development user
-- GRANT ALL PRIVILEGES ON DATABASE abctemplate TO devuser;

-- Display completion message
SELECT 'PostgreSQL initialization completed successfully' AS message;