`AspNetUsers Table`
```sql
-- Custom Identity User Table
CREATE TABLE AspNetUsers (
    -- Primary Key
    Id VARCHAR(255) PRIMARY KEY,
    
    -- Custom Fields
    GithubUserName VARCHAR(100) NULL,
    CreatedDate DATETIME(6) NOT NULL,

    -- Core Authentication (The bare minimum)
    UserName VARCHAR(256) NULL,
    NormalizedUserName VARCHAR(256) NULL,
    Email VARCHAR(256) NULL,
    NormalizedEmail VARCHAR(256) NULL,
    PasswordHash LONGTEXT NULL,
    
    -- SecurityStamp: Invalidates cookies/tokens when password changes
    SecurityStamp LONGTEXT NULL,

    -- ConcurrencyStamp: Prevents two admins from editing a user at the same time
    ConcurrencyStamp LONGTEXT NULL,

    -- Indexes (Essential for fast login lookups)
    UNIQUE KEY UserNameIndex (NormalizedUserName),
    KEY EmailIndex (NormalizedEmail)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
````

`AspNetRoles Table`
```sql
CREATE TABLE aspnetroles (
  Id VARCHAR(255) PRIMARY KEY,
  Name VARCHAR(256) DEFAULT NULL,
  NormalizedName VARCHAR(256) DEFAULT NULL,
  ConcurrencyStamp LONGTEXT,
  UNIQUE KEY RoleNameIndex (NormalizedName)
) ENGINE=InnoDB;
```

`AspNetUserRoles`
```sql
CREATE TABLE aspnetuserroles (
  UserId VARCHAR(255) NOT NULL,
  RoleId VARCHAR(255) NOT NULL,
  PRIMARY KEY (UserId, RoleId),
  CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) 
    REFERENCES aspnetroles (Id) ON DELETE CASCADE,
  CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) 
    REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`Genres Table`
```sql
CREATE TABLE Genres (
    Id INT  AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB;
```

`Tags Table`
```sql
CREATE TABLE Tags (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
) ENGINE=InnoDB;
```

`Engines Table`
```sql
CREATE TABLE Engines (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    Name VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB;
```

`Platforms Table`
```sql
CREATE TABLE Platforms (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB;
```

`Games Table`
```sql
-- Main Entities
CREATE TABLE Games (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    ReleaseDate DATETIME(6) NOT NULL,
    CoverImagePath VARCHAR(500) NOT NULL,
    DownloadLink VARCHAR(500) NOT NULL,
    IsFeatured TINYINT(1) NOT NULL DEFAULT 0,
    CreatedDate DATETIME(6) NOT NULL,
    GenreId INT NOT NULL,
    DeveloperId VARCHAR(255) NOT NULL,
    EngineId INT NOT NULL,
    CONSTRAINT FK_Games_Genres FOREIGN KEY (GenreId) 
        REFERENCES Genres(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Games_Users FOREIGN KEY (DeveloperId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Games_Engines FOREIGN KEY (EngineId) 
        REFERENCES Engines(Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`GameTags Table`
```sql
-- Many-to-Many Join Table for Games and Tags
CREATE TABLE GameTag (
    GameId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (GameId, TagId),
    CONSTRAINT FK_GameTag_Games_GameId FOREIGN KEY (GameId) 
        REFERENCES Games (Id) ON DELETE CASCADE,
    CONSTRAINT FK_GameTag_Tags_TagId FOREIGN KEY (TagId) 
        REFERENCES Tags (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`Reviews Table`
```sql
CREATE TABLE Reviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Rating INT NOT NULL,
    Comment VARCHAR(1000) NOT NULL,
    ReviewDate DATETIME(6) NOT NULL,
    GameId INT NOT NULL,
    UserId VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Reviews_Games_GameId FOREIGN KEY (GameId) 
        REFERENCES Games (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_AspNetUsers_UserId FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`GamePlatforms Table`
```sql
CREATE TABLE GamePlatforms (
    GameId INT NOT NULL, PlatformId INT NOT NULL,
    PRIMARY KEY (GameId, PlatformId),
    CONSTRAINT FK_GP_Games FOREIGN KEY (GameId) 
        REFERENCES Games(Id) ON DELETE CASCADE,
    CONSTRAINT FK_GP_Platforms FOREIGN KEY (PlatformId) 
        REFERENCES Platforms(Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`DownloadHistories Table`
```sql
CREATE TABLE DownloadHistories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DownloadDate DATETIME(6) NOT NULL,
    GameId INT NOT NULL,
    UserId VARCHAR(255) NOT NULL,
    CONSTRAINT FK_DownloadHistories_Games_GameId FOREIGN KEY (GameId) 
        REFERENCES Games (Id) ON DELETE CASCADE,
    CONSTRAINT FK_DownloadHistories_AspNetUsers_UserId FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`Wishlists Table`
```sql
CREATE TABLE Wishlists (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CreatedDate DATETIME(6) NOT NULL,
    GameId INT NOT NULL,
    UserId VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Wishlists_Games_GameId FOREIGN KEY (GameId) 
        REFERENCES Games (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlists_AspNetUsers_UserId FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

`Screenshots Table`
```sql
CREATE TABLE Screenshots (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ImagePath VARCHAR(500) NOT NULL,
    GameId INT NOT NULL,
    CONSTRAINT FK_Screenshots_Games_GameId FOREIGN KEY (GameId) 
        REFERENCES Games (Id) ON DELETE CASCADE
) ENGINE=InnoDB;
```

