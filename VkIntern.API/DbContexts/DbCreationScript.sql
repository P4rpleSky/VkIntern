DROP TABLE IF EXISTS user_group CASCADE;
CREATE TABLE user_group
(
    id serial PRIMARY KEY,
    code varchar(16) UNIQUE NOT NULL,
    description varchar(128)
);

INSERT INTO user_group(code, description)
VALUES
('Admin', 'I am admin!'),
('User', 'I am default user!');



DROP TABLE IF EXISTS user_state CASCADE;
CREATE TABLE user_state
(
    id serial PRIMARY KEY,
    code varchar(16) UNIQUE NOT NULL,
    description varchar(128)
);

INSERT INTO user_state(code, description)
VALUES
('Active', 'This user is active!'),
('Blocked', 'This user is blocked!');


DROP TABLE IF EXISTS "user" CASCADE;
CREATE TABLE "user"
(
    id serial PRIMARY KEY,
	login varchar(16) UNIQUE NOT NULL,
	password varchar(16) NOT NULL,
	created_date date NOT NULL,
	user_group_id int NOT NULL,
    user_state_id int NOT NULL,
	
	CONSTRAINT FK_user_group_id FOREIGN KEY (user_group_id) REFERENCES user_group(id),
	CONSTRAINT FK_user_state_id FOREIGN KEY (user_state_id) REFERENCES user_state(id)
);

INSERT INTO "user"(login, password, created_date, user_group_id, user_state_id)
VALUES
('admin', 'admin123', CURRENT_DATE, 1, 1),
('Alex', '778877', CURRENT_DATE, 2, 1),
('Jon', '11231', CURRENT_DATE, 2, 2);

SELECT * FROM "user";
