function atualizarEstoque() {
    var context = getContext();
    var collection = context.getCollection();

    var linkCollection = collection.getSelfLink();
    var query = "select * from produtos";

    var sucesso = collection.queryDocuments(linkCollection, query, {}, function (err, produtos, responseOptions)
    {
        if (err) throw new Error("Error" + err.message);

        if (produtos.length == 0) throw "Não existem produtos";

        for(i = 0 ; i < produtos.length ; i++)
        {
            produtos[i].quantidade = 500;

            var ok = collection.replaceDocument(produtos[i]._self, produtos[i], function (err2, p) {
                if (err2) throw "Não foi possivel atualizar o produto";
            });
        }
    });

    if (!sucesso) throw "Não foi possível executar a query";
}