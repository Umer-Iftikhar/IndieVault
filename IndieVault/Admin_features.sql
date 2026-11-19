use indiegameplatform;
-- ------------ Admin Ka Kaam --------------

SELECT g.Title, COUNT(*) as WishlistCount
FROM Wishlists w
INNER JOIN Games g ON w.GameId = g.Id
GROUP BY g.Title
ORDER BY WishlistCount DESC
LIMIT 1;

SELECT 
    r.Name AS RoleName, 
    COUNT(ur.UserId) AS TotalUsers
FROM AspNetRoles r
INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
INNER JOIN AspNetUsers u ON ur.UserId = u.Id
GROUP BY r.Name;

select g.Title, g.CoverImagePath, gen.Name as GenreName, g.Price, COALESCE(AVG(re.Rating), 0) AS AverageRating, g.Id,
(
	select u.UserName
    from  aspnetusers u
    where u.Id = g.DeveloperId
) AS Developer
from Games g
left Join genres gen ON g.GenreId = gen.Id
Left Join reviews re  ON g.Id = re.GameId 
where g.IsFeatured = true
group by g.Id, g.Title, g.CoverImagePath, gen.Name , g.Price, g.DeveloperId;
