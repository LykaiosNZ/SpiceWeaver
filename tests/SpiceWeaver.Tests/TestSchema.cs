using Newtonsoft.Json;

namespace SpiceWeaver.Tests;

public static class TestSchema
{
    public const string SpiceDb = """
                                 definition user {}

                                 definition document {
                                  relation viewer: user
                                  relation editor: user
                                  
                                  permission view = viewer + editor
                                  permission edit = editor
                                 }
                                 """;

    public static readonly Schema Object = JsonConvert.DeserializeObject<Schema>(Json)!;

    public const string Json = """
                                     {
                                       "definitions": [
                                         {
                                           "name": "user"
                                         },
                                         {
                                           "name": "document",
                                           "relations": [
                                             {
                                               "name": "viewer",
                                               "types": [
                                                 {
                                                   "type": "user"
                                                 }
                                               ]
                                             },
                                             {
                                               "name": "editor",
                                               "types": [
                                                 {
                                                   "type": "user"
                                                 }
                                               ]
                                             }
                                           ],
                                           "permissions": [
                                             {
                                               "name": "view",
                                               "userSet": {
                                                 "operation": "union",
                                                 "children": [
                                                   {
                                                     "relation": "viewer"
                                                   },
                                                   {
                                                     "relation": "editor"
                                                   }
                                                 ]
                                               }
                                             },
                                             {
                                               "name": "edit",
                                               "userSet": {
                                                 "operation": "union",
                                                 "children": [
                                                   {
                                                     "relation": "editor"
                                                   }
                                                 ]
                                               }
                                             }
                                           ]
                                         }
                                       ]
                                     }
                                     """;
}