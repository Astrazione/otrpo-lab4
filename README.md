# vk_user_relations

OTRPO lab4

Приложение написано на языке C# с использованием `.NET 8.0`


## Рекомендуемый способ запуска

1. Перейти в последний Resease, скачать архив с соответвующей платформой, на которой будет запускаться приложение.
2. Распаковать архив
3. В командной строке указать исполняемый файл и аргументы для запуска

#### Входные параметры приложения

- `--access-token` или `-t` - токен для доступа к `VK Api` (vk1.a...)
- `--user-id` или `-u` - id пользователя, от которого начнётся поиск подписчиков/подписок (default `astraz1one`)
- `--logging-level` или `-l` - уровень логирования приложения без учёта регистра (`None`, `ErrorsOnly`, `Warnings`, default `Full`)
- `--uri` - строка подключения к neo4j (default ``)
- `--neo-login` - логин для базы данных neo4j (default `neo4j`)
- `--neo-password` - пароль для базы данных neo4j (default `password`)
- `--query` или `-q` - тип запроса к графовой БД без учёта регистра (`UsersCount`, `GroupsCount`, `Top5FollowersUsers`, `Top5PopGroups`, `MutualFollowers`, default `NoQuery`)
- `--path` или `-p` - путь для сохранения результатов запроса, если указан log, тогда результат будет выведен в логи (default `log`)
- `--requests-per-second` или `-r` - количетсво запросов в секунду к VK Api (default `3`)

Обязательным параметром является Access Token. Если указать тип запроса не NoQuery, 
тогда токен можно не указывать, так как доступ к vk api не будет осуществляться

Не рекомендуется изменять исходный параметр количества запросов в секунду, так как это стандартное ограничения для токена `VK Api v5.199`

Пример:

запрос на количество групп
```sh
vk_user_relations.exe --query GroupCount --neo-login neo4j --neo-password password --path C:/some_path/result.json
```

С vk id пользователя
```sh
vk_user_relations.exe --access-token ВАШ-ТОКЕН --user-id ID-ПОЛЬЗОВАТЕЛЯ
```

С vk user id и путём для сохранения результата
```sh
vk_user_relations.exe --access-token ВАШ-ТОКЕН --user-id ID-ПОЛЬЗОВАТЕЛЯ
```

### Необходимые пакеты для запуска приложения (для Linux)

https://github.com/dotnet/core/blob/main/release-notes/8.0/linux-packages.md

### Для Windows 10/11 дополнительных пакетов устанавливать не требуется

## Дополнительная информация

В программе используется второй уровень углубления (подписчики пользователя и подписчики подписчиков). Для составления дампа был использован 3 уровень.

По непонятным причинам при попытке записи в базу данных связей в 300 ассинхронных операций база данных падает (хотя добавление узлов работает на все 500 тасков).
Из-за этого для ограничения нагрузки был установлен семафор на 20 одновременных ассинхронных операций.

### Установка пакетов для сборки из исходного кода (альтернативный способ установки)

#### Для Debian

В первой команде подставьте версию Debian (12, 11, 10)
```sh
wget https://packages.microsoft.com/config/debian/ВЕРСИЯ-DEBIAN/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
sudo apt-get install -y dotnet-runtime-8.0
```

#### Для Ubuntu
```sh
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
sudo apt-get install -y dotnet-runtime-8.0
```

#### Для Windows

Скачать и установить .NET SDK 8.0 (`https://dotnet.microsoft.com/en-us/download/dotnet/8.0`)

### Сборка и запуск проекта
Запуск осуществляется с помощью команды `dotnet run` в директории репозитория

### Или сборка исходного кода в приложение (исполняемый файл)

```
dotnet publish -c Release -r <RID> --self-contained true
```

#### Для Windows

RID: `win-x64` 

#### Для Linux

RID: `linux-x64` 