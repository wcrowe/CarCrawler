truncate table emailbatch
go
delete from email
go
delete from car
go
dbcc checkident(email, reseed, 1)
go
dbcc checkident(car, reseed, 1)
go