# xiigen-platform-dot-net-angular::Pbkdf2PasswordHasherService.HashAsync

## Meaning
xiigen-platform-dot-net-angular::Pbkdf2PasswordHasherService.HashAsync implements local project behavior with return type Task<DataProcessResult<PasswordHashResult>>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Auth.Tests\PasswordHasherTests.cs:8::xiigen-platform-dot-net-angular::PasswordHasherTests.HashAsync_StoresNoPlaintext_And_VerifyMatches.

## Location
src\Xiigen.Auth\PasswordHashing.cs:31

## Parameters
- string plaintext
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<PasswordHashResult>> returned by xiigen-platform-dot-net-angular::Pbkdf2PasswordHasherService.HashAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Auth.Tests\PasswordHasherTests.cs:8::xiigen-platform-dot-net-angular::PasswordHasherTests.HashAsync_StoresNoPlaintext_And_VerifyMatches

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Auth\PasswordHashing.cs:31::xiigen-platform-dot-net-angular::Pbkdf2PasswordHasherService.HashAsync