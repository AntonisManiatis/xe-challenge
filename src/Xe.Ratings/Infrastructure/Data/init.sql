CREATE DATABASE "xe_ratings";

\c "xe_ratings";

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS ratee(
    id uuid PRIMARY KEY,
    calculated_at TIMESTAMP,
    total DOUBLE PRECISION DEFAULT 0
);

CREATE TABLE IF NOT EXISTS rating(
    id int GENERATED ALWAYS AS IDENTITY,
    ratee_id uuid NOT NULL,
    rater_id uuid,
    value DOUBLE PRECISION DEFAULT 0,
    posted_at TIMESTAMP NOT NULL
);