namespace Framework

module Database =
    open Dapper
    open System.Data
    open Microsoft.Data.SqlClient

    let slqConBuilder () =
        let builder = SqlConnectionStringBuilder()
        //connection parameters
        new SqlConnection(builder.ConnectionString)

    let mapRows (reader:IDataReader) =
        [
            while reader.Read() do
                yield reader.GetValue(0) |> string
        ]

    let sqlQuery (queryString:string) =
        use conn = slqConBuilder()
        conn.ExecuteReader(queryString) |> mapRows

    let sqlQueryScalar (queryString:string) =
        use conn = slqConBuilder()
        conn.ExecuteScalar(queryString) |> string

    let sqlNonQuery (queryString:string) =
        use conn = slqConBuilder()
        conn.ExecuteReader(queryString) |> ignore