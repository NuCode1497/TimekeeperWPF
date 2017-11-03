
--drop database TimekeeperDB
--create database TimekeeperDB


--use TimeKeeperDB

--insert into Notes (DateTime, Text)
--values (GETDATE(), 'Test note please ignore.');

--insert into TaskTypes (Name) values ('Note')
--insert into TaskTypes (Name) values ('Work')
--insert into TaskTypes (Name) values ('Chore')
--insert into TaskTypes (Name) values ('Play')
--insert into TaskTypes (Name) values ('Eat')
--insert into TaskTypes (Name) values ('Sleep')
--insert into TaskTypes (Name) values ('DBTest')

----DBCC CHECKIDENT ('[TaskTypes]', RESEED, 0);

----update TaskTypes set Name = Note where Id = 1