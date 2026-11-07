import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import prettierRecommended from "eslint-plugin-prettier/recommended";

export default defineConfig([
  ...nextVitals,
  prettierRecommended,
  {
    rules: {
      "react-hooks/exhaustive-deps": "off",
      "prettier/prettier": "warn",
      // New in eslint-plugin-react-hooks 7; existing code predates these rules.
      "react-hooks/set-state-in-effect": "warn",
      "react-hooks/immutability": "warn",
    },
  },
  globalIgnores([".next/**", "out/**", "build/**", "next-env.d.ts"]),
]);
