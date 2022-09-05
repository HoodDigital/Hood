IF EXISTS(select * FROM sys.views where name = 'HoodContentViews') DROP VIEW HoodContentViews
GO
CREATE VIEW HoodContentViews AS
SELECT  
	HoodContent.Id,
	HoodContent.AllowComments,
	HoodContent.AuthorId,
	HoodContent.Body,
	HoodContent.ContentType,
	HoodContent.CreatedBy,
	HoodContent.CreatedOn,
	HoodContent.Excerpt,
	HoodContent.FeaturedImageJson,
	HoodContent.LastEditedBy,
	HoodContent.LastEditedOn,
	HoodContent.Notes,
	HoodContent.ParentId,
	HoodContent.[Public],
	HoodContent.PublishDate,
	HoodContent.ShareCount,
	HoodContent.Slug,
	HoodContent.[Status],
	HoodContent.SystemNotes,
	HoodContent.Title,
	HoodContent.UserVars,
	HoodContent.Views,
	HoodContent.Featured,
	HoodContent.ShareImageJson,
	AspNetUsers.FirstName,
	AspNetUsers.LastName,
	AspNetUsers.AvatarJson,
	AspNetUsers.Anonymous,
	AspNetUsers.DisplayName,
	AspNetUsers.Email as AuthorEmail, 
	AspNetUsers.UserVars as AuthorVars
FROM
	HoodContent JOIN
	AspNetUsers ON HoodContent.AuthorId = AspNetUsers.Id
GO