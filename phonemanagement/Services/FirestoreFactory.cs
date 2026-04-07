using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;

namespace phonemanagement.Services;

public static class FirestoreFactory
{
    public static FirestoreDb Create(IConfiguration config)
    {
        var projectId = config["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
            throw new InvalidOperationException("Missing Firebase:ProjectId in configuration.");

        var credentialsPath = config["Firebase:CredentialsPath"];
        if (!string.IsNullOrWhiteSpace(credentialsPath))
        {
            // Uses the supported factory API (avoids deprecated GoogleCredential.FromFile).
            var credential = CredentialFactory.FromFile(credentialsPath, "service_account");
            return new FirestoreDbBuilder
            {
                ProjectId = projectId,
                GoogleCredential = credential
            }.Build();
        }

        // Falls back to default credentials (e.g. GOOGLE_APPLICATION_CREDENTIALS env var)
        return FirestoreDb.Create(projectId);
    }
}

