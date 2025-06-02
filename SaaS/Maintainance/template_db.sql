CREATE DATABASE template_db
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
	TEMPLATE = template0
	IS_TEMPLATE = false;

CREATE TABLE IF NOT EXISTS users
(
    username varchar(64) unique not null,
    password varchar(300) not null,
	role varchar(32) not null
)

UPDATE pg_database SET datistemplate = true WHERE datname = 'template_db';