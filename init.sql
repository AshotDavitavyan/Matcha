CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50),
    email VARCHAR(50),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    password VARCHAR(100)
);