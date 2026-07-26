using System.Text.Json;
using api.Contracts;

namespace api.Services;

public static class StockTechTrainingProtocol
{
    public const string ContractVersion = "1.0";
    public const string ModelName = "StockTechAI";

    public static AiTrainResponse Validate(AiTrainResponse response)
    {
        if (!ValidIdentity(response) || !ValidOutcome(response))
        {
            throw new InvalidOperationException("StockTechAI returned an unsupported training response.");
        }
        return response;
    }

    private static bool ValidIdentity(AiTrainResponse response)
    {
        return response.ModelName == ModelName
            && response.ContractVersion == ContractVersion
            && !string.IsNullOrWhiteSpace(response.ModelVersion)
            && response.ModelVersion.Length <= 64
            && !response.IsMock;
    }

    private static bool ValidOutcome(AiTrainResponse response)
    {
        return Completed(response) || Insufficient(response);
    }

    private static bool Completed(AiTrainResponse response)
    {
        return response.Status == "completed"
            && response.Trained
            && response.Artifact is { ValueKind: JsonValueKind.Object }
            && response.Metrics is { ValueKind: JsonValueKind.Object };
    }

    private static bool Insufficient(AiTrainResponse response)
    {
        return response.Status == "insufficient-data"
            && !response.Trained
            && response.Artifact is null
            && response.Metrics is null;
    }
}
