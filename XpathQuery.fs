namespace Framework

module XpathQuery =
    type AttributeType =
        | Id of string
        | Class of string
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
        | Text a -> sprintf "contains(text(), '%s')" a
        | InnerNode node -> sprintf ".%s" node
        | DataHashLocation a -> sprintf "contains(concat(' ', normalize-space(@data-hash-location), ' '), ' %s ')" a
        | NodeIndex a -> sprintf "%i" a

    let where attType node =
        sprintf "%s[%s]" node (contains attType)

    let xPathForJs query = 
        query |> sprintf "document.evaluate( \"%s\", document, null, XPathResult.ANY_TYPE, null ).iterateNext()"