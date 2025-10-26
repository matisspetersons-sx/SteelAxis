# Security Advisory: Microsoft.Identity.Web Package

## Known Vulnerability

The Microsoft.Identity.Web package (all versions in the 3.x series) has a known **moderate severity** vulnerability:

- **CVE**: GHSA-rpq8-q44m-2rpg
- **Severity**: Moderate
- **Affected Versions**: Microsoft.Identity.Web 3.x
- **Details**: https://github.com/advisories/GHSA-rpq8-q44m-2rpg

## Current Status

- **Installed Version**: 3.6.0 (latest stable in 3.x series)
- **Risk Assessment**: Moderate
- **Mitigation Status**: Microsoft is aware of this issue

## Recommendations

### Short-term (Current Implementation)
1. ✅ Using the latest 3.x version (3.6.0) which includes other security fixes
2. ✅ Following security best practices:
   - HTTPS enforcement
   - Secure token storage
   - Proper CORS configuration
   - Regular security updates

### Long-term (Production Deployment)
1. **Monitor for Updates**
   - Watch for Microsoft.Identity.Web 4.x or patched versions
   - Subscribe to security advisories: https://github.com/AzureAD/microsoft-identity-web/security/advisories

2. **Upgrade When Available**
   ```bash
   # When fixed version is released:
   dotnet add package Microsoft.Identity.Web --version <fixed-version>
   dotnet add package Microsoft.Identity.Web.UI --version <fixed-version>
   ```

3. **Alternative Approach**
   - Consider using Microsoft Entra External ID with native .NET authentication if vulnerability is critical for your use case
   - Evaluate moving to .NET 9 when released (may include updated packages)

## Workarounds

While waiting for a fix:

1. **Network Security**
   - Use Web Application Firewall (WAF)
   - Implement DDoS protection
   - Enable Azure Front Door or Application Gateway

2. **Authentication Hardening**
   - Enable Multi-Factor Authentication (MFA)
   - Implement Conditional Access policies
   - Use short-lived tokens
   - Implement token binding where possible

3. **Monitoring**
   - Enable Azure AD sign-in logs
   - Set up alerts for suspicious authentication attempts
   - Monitor token usage patterns
   - Implement audit logging

## Verification Commands

Check current package version:
```bash
dotnet list package | grep Microsoft.Identity.Web
```

Check for package updates:
```bash
dotnet list package --outdated
```

## References

- [Microsoft.Identity.Web GitHub](https://github.com/AzureAD/microsoft-identity-web)
- [Security Advisories](https://github.com/AzureAD/microsoft-identity-web/security/advisories)
- [NuGet Package](https://www.nuget.org/packages/Microsoft.Identity.Web)
- [Microsoft Security Response Center](https://msrc.microsoft.com/)

## Updates

**Last Checked**: October 26, 2025
**Action Required**: Monitor for updates, implement recommended security practices
**Risk Level**: Acceptable for development; Review for production deployment

---

*This file should be reviewed monthly and updated when new security information becomes available.*
