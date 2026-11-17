use indiegameplatform;

-- Display 10 games on each page , along with their genre, Develper and their average ratings per game.
Select g.Title, g.Price, g.ReleaseDate, gen.Name, COALESCE(AVG(re.Rating), 0) AS AverageRatings, g.Id, g.CoverImagePath,
-- COALESCE handles null exceptions when there are no reviews
( -- subquery returns dev name
	select u.UserName
    from  aspnetusers u
    where u.Id = g.DeveloperId
) AS Developer
from games g 
left Join genres gen ON g.GenreId = gen.Id
Left Join reviews re  ON g.Id = re.GameId 
WHERE (1 IS NULL OR g.GenreId = 1) -- filtering genre
AND (g.Price BETWEEN 0 AND 40) -- filtering price
AND (g.Title LIKE '%Sleek%')
AND EXISTS 
(                                         -- 3. Platform Filter
	SELECT 1 
	FROM gameplatforms gp 
	WHERE gp.GameId = g.Id AND gp.PlatformId = 2
)
GROUP BY g.Title, g.Price, g.ReleaseDate, gen.Name, g.DeveloperId, g.Id, g.CoverImagePath
-- 4. ORDER BY CASE: sorting logic
ORDER BY 
    -- First priority: Newest
    CASE WHEN @SortBy = 'Newest' THEN g.ReleaseDate END DESC,
    -- Second priority: Price
    CASE WHEN @SortBy = 'PriceAsc' THEN g.Price END ASC,
    -- Third priority: Title 
    CASE WHEN @SortBy IS NULL THEN g.Title END ASC
limit 10 -- only 10 games per page
offset 0; -- skip 10 games on next page


-- Simulate parameters by hardcoding values for testing
-- Change these to NULL or different values to test different scenarios
SET @GenreId = 1;
SET @MinPrice = 0;
SET @MaxPrice = 40;
SET @SearchTerm = 'Sleek';
SET @PlatformId = 2; -- Set to NULL to test "all platforms"
SET @SortBy = 'Rating'; -- Options: 'Newest', 'PriceAsc', 'PriceDesc', 'Rating'

SELECT  g.Title, g.Price, g.ReleaseDate, gen.Name AS GenreName, COALESCE(AVG(re.Rating), 0) AS AverageRatings, g.Id, g.CoverImagePath,
(
	SELECT u.UserName
	FROM aspnetusers u
	WHERE u.Id = g.DeveloperId
) AS Developer
FROM games g 
LEFT JOIN genres gen ON g.GenreId = gen.Id
LEFT JOIN reviews re ON g.Id = re.GameId 
WHERE (@GenreId IS NULL OR g.GenreId = @GenreId)
AND (@MinPrice IS NULL OR g.Price >= @MinPrice)
AND (@MaxPrice IS NULL OR g.Price <= @MaxPrice)
AND (@SearchTerm IS NULL OR g.Title LIKE CONCAT('%', @SearchTerm, '%'))
AND (@PlatformId IS NULL OR EXISTS 
(
	SELECT 1 
	FROM gameplatforms gp 
	WHERE gp.GameId = g.Id AND gp.PlatformId = @PlatformId
))
GROUP BY g.Title, g.Price, g.ReleaseDate, gen.Name, g.DeveloperId, g.Id, g.CoverImagePath
ORDER BY 
    -- 1. Rating (Highest first)
    CASE WHEN @SortBy = 'Rating' THEN COALESCE(AVG(re.Rating), 0) END DESC,
    -- 2. Price Descending
    CASE WHEN @SortBy = 'PriceDesc' THEN g.Price END DESC,
    -- 3. Price Ascending
    CASE WHEN @SortBy = 'PriceAsc' THEN g.Price END ASC,
    -- 4. Newest (Release Date)
    CASE WHEN @SortBy = 'Newest' THEN g.ReleaseDate END DESC,
    -- Default/Tie-breaker
    g.Title ASC
LIMIT 10 OFFSET 0;

SET @GenreId = NULL;
SET @MinPrice = null;
SET @MaxPrice = null;
SET @SearchTerm = null;
SET @PlatformId = null;

SELECT COUNT(DISTINCT g.Id) AS TotalFilteredGames
FROM games g 
WHERE 
    (@GenreId IS NULL OR g.GenreId = @GenreId)
    AND (@MinPrice IS NULL OR g.Price >= @MinPrice)
    AND (@MaxPrice IS NULL OR g.Price <= @MaxPrice)
    AND (@SearchTerm IS NULL OR g.Title LIKE CONCAT('%', @SearchTerm, '%'))
    AND (@PlatformId IS NULL OR EXISTS (
        SELECT 1 
        FROM gameplatforms gp 
        WHERE gp.GameId = g.Id AND gp.PlatformId = @PlatformId
    ));

-- Fixing coverimage

UPDATE Games SET CoverImagePath = CONCAT('https://picsum.photos/seed/', Id, '/800/600');
SELECT Id, CoverImagePath FROM Games LIMIT 5;
SELECT CoverImagePath FROM Games LIMIT 3;

SELECT COUNT(*) FROM Games WHERE CoverImagePath LIKE '%placeholder%';

UPDATE Games 
SET CoverImagePath = CONCAT('https://picsum.photos/seed/', Id, '/800/600')
WHERE CoverImagePath LIKE '%placeholder%';

SET SQL_SAFE_UPDATES = 0;
UPDATE Games SET CoverImagePath = CONCAT('https://picsum.photos/seed/', Id, '/800/600') WHERE CoverImagePath LIKE '%placeholder%';
SET SQL_SAFE_UPDATES = 1;

UPDATE Games SET CoverImagePath = CONCAT('https://picsum.photos/seed/', Id, '/800/600') WHERE CoverImagePath LIKE '%placeholder%';
SET SQL_SAFE_UPDATES = 1;
