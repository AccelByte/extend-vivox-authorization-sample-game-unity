// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class ExtensionMethods
{
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOperation)
    {
        TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

        asyncOperation.completed += obj =>
        {
            taskCompletionSource.SetResult(null);
        };

        return ((Task)taskCompletionSource.Task).GetAwaiter();
    }
}
