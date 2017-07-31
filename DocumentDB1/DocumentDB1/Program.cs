using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDB1
{
    class Program
    {
        private const string endpoint = "https://nosqlgroup.documents.azure.com:443/";
        private const string primaryKey = "O1VuX2UoDujw4yy7eL32MlREQdFY0rjBV54kJLazNwuZKuyLwM2Le5a1BlaJlfW1M16TMje742SIX3fkzeNXSg==";

        static DocumentClient client = new DocumentClient(new Uri(endpoint), primaryKey);
        
        static void Main(string[] args)
        {
            Database db = CriarDataBase("ecommerce").Result;

            DocumentCollection produtos = CriarColecaoDocumento("produtos", db).Result;

            CriarProdutos(produtos.SelfLink);

            ListarLivrosMSPress(produtos);

            Console.ReadLine();

            StoredProcedure proc = CriarProcedure(produtos).Result;

            var result = ExecutarProcedure(proc).Result;

            ListarQuantidadeEmEstoque(produtos);

            Console.ReadLine();
        }

        private static async Task<Database> CriarDataBase(string databaseId)
        {
            var db = client.CreateDatabaseQuery().Where(x => x.Id == databaseId).AsEnumerable().FirstOrDefault();

            if (db != null)
                return db;

            return await client.CreateDatabaseAsync(new Database()
            {
                Id = databaseId
            });
        }

        private static async Task<DocumentCollection> CriarColecaoDocumento(string colecaoId, Database db)
        {
            var col = client.CreateDocumentCollectionQuery(db.SelfLink)
                            .Where(x => x.Id == colecaoId)
                            .ToArray()
                            .FirstOrDefault();

            if(col == null)
            {
                col = await client.CreateDocumentCollectionAsync(db.SelfLink, new DocumentCollection
                {
                    Id = colecaoId
                });
            }
            return col;
        }

        private static async void CriarProdutos(string colecaoSelfLink)
        {
            dynamic azure = new { 
                id = "000001",
                titulo = "Microsoft Azure",
                editora = new
                {
                    id = "1",
                    nome = "MS Press"
                },
                quantidade = 100,
                preco = 49,
                autor = new
                {
                    id = "1",
                    nome = "Jhonathan Soares"
                }
            };

            await client.CreateDocumentAsync(colecaoSelfLink, azure);

            dynamic documentDB = new {
                id = "000002",
                titulo = "Azure DocumentDB",
                editora = new
                {
                    id = "1",
                    nome = "MS Press"
                },
                quantidade = 2,
                autor = new
                {
                    id = "1",
                    nome = "Jhonathan Soares"
                }
            };

            await client.CreateDocumentAsync(colecaoSelfLink, documentDB);
        }

        private static void ListarLivrosMSPress(DocumentCollection produtos)
        {
            var livros = client.CreateDocumentQuery(produtos.SelfLink, "select * from produtos p where p.editora.nome = \"MS Press\"").ToList();

            foreach (var livro in livros)
            {
                Console.WriteLine(livro.titulo);
            }
        }

        private static async Task<StoredProcedure> CriarProcedure(DocumentCollection produtos)
        {
            StoredProcedure spAtualizaEstoque = new StoredProcedure
            {
                Id = "spAtualizaEstoque",
                Body = File.ReadAllText(@"C:\Users\jhonathan.soares\Desktop\AzureDocumentDB-master\DocumentDB1\DocumentDB1\atualizaEstoque.js")
            };

            return await client.CreateStoredProcedureAsync(produtos.SelfLink, spAtualizaEstoque);
        }

        private static async Task<bool> ExecutarProcedure(StoredProcedure proc)
        {
            await client.ExecuteStoredProcedureAsync<dynamic>(proc.SelfLink, null);

            return true;
        }

        private static void ListarQuantidadeEmEstoque(DocumentCollection produtos)
        {
            var livros = client.CreateDocumentQuery(produtos.SelfLink, "select * from produtos").ToList();

            foreach (var livro in livros)
            {
                Console.WriteLine(livro.quantidade);
            }
        }
    }
}
