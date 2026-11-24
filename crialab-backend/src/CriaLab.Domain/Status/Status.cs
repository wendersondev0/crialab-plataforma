namespace CriaLab.Domain.Status;

public static class StatusPedido
{
    public const string Novo = "novo";
    public const string AguardandoPagamento = "aguardando pagamento";
    public const string Pago = "pago";
    public const string EmProducao = "em produção";
    public const string Enviado = "enviado";
    public const string Entregue = "entregue";
    public const string Cancelado = "cancelado";
    public const string Devolvido = "devolvido";

    public static readonly string[] Todos =
    [
        Novo,
        AguardandoPagamento,
        Pago,
        EmProducao,
        Enviado,
        Entregue,
        Cancelado,
        Devolvido
    ];
}

public static class StatusOs
{
    public const string Aguardando = "aguardando";
    public const string EmProducao = "em producao";
    public const string Concluida = "concluida";

    public static readonly string[] Todos = [Aguardando, EmProducao, Concluida];
}

public static class StatusOrcamento
{
    public const string Aguardando = "aguardando";
    public const string Respondido = "respondido";
    public const string Aceito = "aceito";
    public const string Recusado = "recusado";

    public static readonly string[] Todos = [Aguardando, Respondido, Aceito, Recusado];
}

public static class StatusMensagem
{
    public const string Aberto = "aberto";
    public const string EmAtendimento = "em atendimento";
    public const string Resolvido = "resolvido";
    public const string Fechado = "fechado";

    public static readonly string[] Todos = [Aberto, EmAtendimento, Resolvido, Fechado];
}

public static class StatusImpressao
{
    public const string NaFila = "na fila";
    public const string Imprimindo = "imprimindo";
    public const string Pausada = "pausada";
    public const string Erro = "erro";
    public const string Concluida = "concluida";
    public const string Cancelada = "cancelada";

    public static readonly string[] Todos = [NaFila, Imprimindo, Pausada, Erro, Concluida, Cancelada];
}

public static class StatusProduto
{
    public const string Publicado = "publicado";
    public const string Pausado = "pausado";

    public static readonly string[] Todos = [Publicado, Pausado];
}

public static class StatusMaquina
{
    public const string Ativa = "ativa";
    public const string Manutencao = "manutencao";
    public const string Parada = "parada";
    public const string Baixada = "baixada";

    public static readonly string[] Todos = [Ativa, Manutencao, Parada, Baixada];
}
