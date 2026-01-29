# coding-standards.md

## General
- Use **PascalCase** for classes, methods, properties.
- Use **camelCase** for local variables and parameters.
- Use **_underscoreCamelCase** for private fields.

## Architecture Guidelines (DCM)
- **Services**: Must be stateless. Depend only on Interfaces/Models.
- **Managers**: Orchestrators. Manage UOW lifecycle.
- **Configurators**: Only place where `new ConcreteImplementation()` is allowed for Dependencies.
- **Interfaces**: Place in `Veradyn.Core.Interfaces`. No external lib dependencies in interfaces if possible.

## Documentation
- All public members must have XML summary comments.
- Complex protocol logic must explain the "Why" (e.g. referencing RFC section).

## Security
- Do not log PII.
- Validate all inputs in the Manager layer.
- Use `SystemCryptoProvider` for hashing.
