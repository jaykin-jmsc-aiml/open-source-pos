# Quick Fixes for Phase 1 Issues

## Issue 1: Frontend TypeScript Unused Variables

### File: `src/Frontend/src/App.tsx`

**Line 9**: Variable `services` is declared but never used

**Current Code:**
```typescript
const [services, setServices] = useState<ServiceStatus[]>([])
```

**Option A (Preferred)**: Prefix with underscore to indicate intentionally unused
```typescript
const [_services, setServices] = useState<ServiceStatus[]>([])
```

**Option B**: Remove the variable entirely if not needed
```typescript
const [, setServices] = useState<ServiceStatus[]>([])
```

---

### File: `src/Frontend/src/test/setup.ts`

**Line 1**: Import `expect` is not used directly

**Current Code:**
```typescript
import { expect, afterEach } from 'vitest';
```

**Fix**: Remove `expect` from import (it's available globally via jest-dom)
```typescript
import { afterEach } from 'vitest';
```

---

## Issue 2: Docker Compose Version Declaration

### File: `docker-compose.yml`

**Line 1**: Version declaration is obsolete in Docker Compose v2+

**Current Code:**
```yaml
version: '3.8'

services:
  sqlserver:
```

**Fix**: Remove the version line entirely
```yaml
services:
  sqlserver:
```

---

## Commands to Verify Fixes

After making the changes:

```bash
# Verify .NET build still works
dotnet build

# Verify .NET tests still pass
dotnet test

# Verify frontend build now works
cd src/Frontend
npm run build

# Verify Docker Compose is valid
cd ../..
docker compose config

# Optional: Build all containers
docker compose build
```

---

## Expected Results After Fixes

✅ `dotnet build` - Should still succeed with 0 errors
✅ `dotnet test` - Should still pass 7/7 tests
✅ `npm run build` - Should now succeed with no TypeScript errors
✅ `docker compose config` - Should show no warnings about version
✅ All Phase 1 diagnostics GREEN

---

## Estimated Time

- Fix 1 (TypeScript): 2 minutes
- Fix 2 (Docker Compose): 1 minute
- Verification: 2 minutes
- **Total: 5 minutes**

---

## Priority

These are the ONLY issues preventing 100% Phase 1 completion.
All other systems are fully functional.
