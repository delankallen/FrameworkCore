namespace Framework

module XpathQuery =
    type AttributeType =
        | Id of string
        | Class of string
        | Title of string
        | Attribute of string * string
        | Text of string
        | InnerNode of string
        | DataHashLocation of string
        | NodeIndex of int

    let select node =
        node |> sprintf "//%s"
    
    let from parent node =
        sprintf "%s%s" parent node

    let private contains attType =
        match attType with
        | Id a -> sprintf "@id='%s'" a
        | Class a  -> sprintf "contains(concat(' ', normalize-space(@class), ' '), ' %s ')" a
        | Title a -> sprintf "@title='%s'" a
        | Attribute (attr, value) -> sprintf "@%s='%s'" attr value
        | Text a -> sprintf "contains(text(), '%s')" a
        | InnerNode node -> sprintf ".%s" node
        | DataHashLocation a -> sprintf "contains(concat(' ', normalize-space(@data-hash-location), ' '), ' %s ')" a
        | NodeIndex a -> sprintf "%i" a

    let where attType node =
        sprintf "%s[%s]" node (contains attType)

    let xPathForJs query = 
        query |> sprintf "document.evaluate( \"%s\", document, null, XPathResult.ANY_TYPE, null ).iterateNext()"

    
    // Sample of what the queries look like
    // module folderTree =
    //   let foldersDiv = select "div" |> where (Id "fileExplorer_treeView")
    //   let subFolders = select "ul" |> where (Class "e-treeview-ul") |> from foldersDiv
    //   let subFolder folderName = select "li" |> where (Id folderName)
    //   let selectSubFolder folderName = select "a" |> from (subFolder folderName)
    //   let subFolderActive folderName = select "a" |> where (Class "e-active") |> from (subFolder folderName)