DO
$do$
BEGIN
   IF EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'app') THEN

      RAISE NOTICE 'Role "app" already exists. Skipping.';
ELSE
CREATE ROLE app LOGIN PASSWORD 'app';
END IF;
END
$do$;

GRANT CONNECT        ON DATABASE "DefaultDbName"   TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA vk TO app;
GRANT ALL PRIVILEGES ON SCHEMA vk                  TO app;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA vk TO app;
