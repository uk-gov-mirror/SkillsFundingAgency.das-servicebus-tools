# das-servicebus-tools

# The SFA.DAS.ServiceBus.Tools.Functions app

The SFA.DAS.ServiceBus.Tools.Function application is a simple Azure hosted Function which allows publishing specific messages via HttpTriggers. Execution via the Azure Portal is recommended for simplicity but you will need to ensure a CORS exeception has been added for 'https://portal.azure.com' beforehand.

The messages which can be published are:

* DraftExpireAccountFunds
* DraftExpireFunds
* ExpireAccountFunds
* ExpireFunds
* ImportAccountLevyDeclarations
* ImportPayments
* ImportAccountPayments
* ProcessPeriodEndPayments
* StoreLearningHistory
* ApprenticeshipEmployerTypeChange
* RefreshEmployerLevyDataCompleted
* LearningWithdrawn
* LearningPaused
* LearningResumed
* ApprovedLearningUpdated

The Functions app targets **.NET 10** (`net10.0`). All HttpTrigger endpoint verbs are POST. The shape of the messges are as shown below:

### DraftExpireAccountFunds

```javascript
{
    "AccountId": "00000",
    "DateTo": "2024-01-01"
}
```
---
### DraftExpireFunds

```javascript
{
    "DateTo": "2024-01-01"
}
```
---
### ExpireAccountFunds

```javascript
{
    "AccountId": "00000",
}
```
---
### ExpireFunds
```javascript
{ 

}
```
---
### ImportAccountLevyDeclarations

```javascript
{
    "AccountId": "00000",
    "PayeRef": "ABC/123245"
}
```
---
### ImportPayments
```javascript
{

}
```
---
### ImportAccountPayments

```javascript
{
    "AccountId": "00000",
    "PeriodEndRef": "2425-R01"
}
```
---
### ProcessPeriodEndPayments

```javascript
{
    "PeriodEndRef": "2324-R10",
    "BatchNumber": "1"
}
```

---
### StoreLearningHistory

Publishes `SFA.DAS.CommitmentsV2.Messages.Commands.StoreLearningHistoryCommand` to `SFA.DAS.CommitmentsV2.MessageHandlers`. The apprenticeship id must exist in that environment.

```javascript
{
    "ApprenticeshipId": 264643,
    "Source": 1,
    "ChangeType": 2,
    "AppliedDate": "2026-06-03T00:00:00",
    "Description": "Testing from NserviceBus"
}
```

**Source** (`LearningSourceType`): `0` Approval API, `1` ILR status change, `2` Manual change.

**ChangeType** (`LearningChangeType`): `0` Auto approved, `1` Rejected, `2` Employer approved, `3` Employer rejected, `4` Manual update.

Optional: `LearningKey` (GUID), `UserId` (GUID).

---
### ApprenticeshipEmployerTypeChange

Publishes `SFA.DAS.EmployerAccounts.Messages.Events.ApprenticeshipEmployerTypeChangeEvent` to the service bus. Use this spike endpoint to simulate employer account levy type changes in lower environments.

```javascript
{
    "AccountId": 12345,
    "ApprenticeshipEmployerType": 1
}
```

**ApprenticeshipEmployerType** (`SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType`): `0` NonLevy, `1` Levy, `2` Unknown.

Optional: `Created` (DateTime).

---
### RefreshEmployerLevyDataCompleted

Publishes `SFA.DAS.EmployerFinance.Messages.Events.RefreshEmployerLevyDataCompletedEvent` to the service bus. Use this to test the employer-accounts `EmployerAccountLevyStatus` projection (APPMAN-2762) without running a full levy import.

Consumed by `SFA.DAS.EmployerAccounts.MessageHandlers` → `RefreshEmployerLevyDataAccountLevyStatusProjectionHandler`.

```javascript
{
    "AccountId": 1001,
    "PayeRef": "123/AB45678",
    "LastLevyDeclarationDate": "2024-03-15T00:00:00",
    "PeriodMonth": 6,
    "PeriodYear": "2526",
    "LevyImported": false,
    "LevyTransactionValue": 0,
    "Created": "2026-06-20T10:00:00Z"
}
```

`LastLevyDeclarationDate` is the field written to `EmployerAccountLevyStatus` and used by the dormancy assessment job. Use a date older than your configured `NoLevyDeclaredMonths` threshold to test dormant-candidate detection, or a recent date to test levy-resume cancellation of active dormancy requests.

`Created` maps to `LastRefreshedAt` on the projection. Older `Created` values are ignored when a newer refresh already exists.

To test a never-declared account (dormancy candidate with no historic declaration), set `LastLevyDeclarationDate` to `null`:

```javascript
{
    "AccountId": 1001,
    "PayeRef": "123/AB45678",
    "LastLevyDeclarationDate": null,
    "PeriodMonth": 6,
    "PeriodYear": "2526",
    "LevyImported": false,
    "LevyTransactionValue": 0,
    "Created": "2026-06-20T10:00:00Z"
}
```

For a full finance import path that also publishes this event, use `ImportAccountLevyDeclarations` instead.

---
### LearningWithdrawn

Publishes `SFA.DAS.Learning.Types.LearningWithdrawnEvent` to the shared service bus. Consumed by `SFA.DAS.CommitmentsV2.ExternalHandlers`, which stops the apprenticeship via ILR withdrawal logic, publishes stop events (`IsWithdrawnViaIlr = true`), and sends `StoreLearningHistoryCommand` automatically. Use `StoreLearningHistory` instead if you only need to test audit history in isolation.

The apprenticeship id must exist in that environment. `WithdrawalDate` must be the 1st of the month and cannot be in the future (unless training has not started, in which case it must equal the start date). Re-publishing for an apprenticeship already withdrawn via ILR may fail validation or produce a stop-date-changed event depending on dates.

```javascript
{
    "ApprenticeshipId": 264643,
    "LearningKey": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "WithdrawalDate": "2026-05-01T00:00:00",
    "WithdrawalReasonCode": 12,
    "Created": "2026-06-11T10:00:00"
}
```

**WithdrawalReasonCode**: ILR withdrawal reason code stored on the apprenticeship. Code `29` sets the redundancy flag.

---
### LearningPaused

Publishes `SFA.DAS.Learning.Types.LearningPausedEvent` to the shared service bus. Consumed by `SFA.DAS.CommitmentsV2.ExternalHandlers`, which pauses the apprenticeship via ILR pause logic, publishes `ApprenticeshipPausedEvent` (`PausedViaILR = true`), and sends `StoreLearningHistoryCommand` automatically. Use `StoreLearningHistory` instead if you only need to test audit history in isolation.

The apprenticeship id must exist in that environment. `PauseDate` must be on or after the apprenticeship start date and before the end date. Learning cannot be paused when payment status is Completed or Withdrawn.

```javascript
{
    "ApprenticeshipId": 264643,
    "LearningKey": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "PauseDate": "2026-05-01T00:00:00",
    "Created": "2026-06-11T10:00:00"
}
```
---
### LearningResumed

Publishes `SFA.DAS.Learning.Types.LearningResumedEvent` to the shared service bus. Consumed by `SFA.DAS.CommitmentsV2.ExternalHandlers`, which resumes the apprenticeship via ILR resume logic, publishes `ApprenticeshipResumedEvent` (`ResumedViaILR = true`), and sends `StoreLearningHistoryCommand` automatically. Use `StoreLearningHistory` instead if you only need to test audit history in isolation.

The apprenticeship id must exist in that environment. `ResumeDate` is after the apprenticeship start date and before the end date and on or after the paused date. Learning cannot be resumed when payment status is Completed or Withdrawn.

```javascript
{
    "ApprenticeshipId": 264643,
    "LearningKey": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "ResumeDate": "2026-06-01T00:00:00",
    "Created": "2026-08-10T10:00:00"
}
```
---
### ApprovedLearningUpdated

Publishes `SFA.DAS.Learning.Types.ApprovedLearningUpdatedEvent` to the shared service bus. Consumed by `SFA.DAS.CommitmentsV2.ExternalHandlers`, which updates the apprenticeship in the CommitmentsV2 DB.

The apprenticeship id must exist in that environment. 

```javascript
{
   "apprenticeshipId": 264643,
  "learningType": "apprenticeship",
  "learningKey": a1b2c3d4-e5f6-7890-abcd-ef1234567890,
  "learningUri": "uri",
  "changes": [
    {
      "changeType": "Firstname",
      "data": {
        "old": "TestFirstname",
        "new": "TestFirstname1"
      }
    },
    {
      "changeType": "Surname",
      "data": {
        "old": "TestSurname",
        "new": "TestSurname1"
      }
    }
  ]
}
```
---



# Running the dead letter message requeue app

DLQConsole application will read all the messages in a dead letter queue and add them back onto the topic queue they where dead lettered from. 

__NOTE:__ The requeue of messages will send the requeue queue message to every subscription even if only one subscription has failed to process the message so it is worth making sure either subscribers can handle multiple duplicate message publishing correcly or there is only one subscription to the topic where the message is being requeued.

### Starting the console application
To run the application you need to start the exe file (preferably from command line if you want the messages to persist on screen).

### Entering the Azure queue details
The dead letter requeue app needs the following details to work correctly:

- Connection string to the azure service bus the dead letter queue exists on
- The topic name of the dead letter queue
- The topic subscription name as the dead letter queue is per subscription

When running the application you will be prompted to enter the above information in the same order.

__NOTE:__ Currently the application has only been tested with connection strings that have read, write and manage permissions.

Once you have entered all the above information when prompted the application will look at each message in the dead letter queue for that topic and subscription and attemp to requeue onto the same topic. You will see console messages for each message processed and will get a conformation message when all processing is completed. All of the messages are fairly obvious so there should be no need to list them here.

### Error handling

Currently the dead letter requeue app has no error handling and this will follow once clearer requirements are worked out. 


