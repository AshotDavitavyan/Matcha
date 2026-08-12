CREATE TYPE gender_type AS ENUM ('male', 'female');

CREATE TYPE sexual_preference_type AS ENUM ('male', 'female', 'both');

CREATE TABLE users (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username VARCHAR(50),
    email VARCHAR(50),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    password VARCHAR(100),
    gender gender_type,
    sexual_preferences sexual_preference_type,
    biography TEXT
);

CREATE TABLE tags (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE user_tags (
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    tag_id INTEGER NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, tag_id)
);

CREATE TABLE user_pictures (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    url VARCHAR(500) NOT NULL,
    is_pfp BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE user_likes (
    liker_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    liked_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    PRIMARY KEY (liker_id, liked_id),
    CHECK (liker_id <> liked_id)
);

CREATE TABLE refresh_tokens (
    user_id INTEGER PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL
);
