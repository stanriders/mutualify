import { createContext } from "react";

const UserContext = createContext({
  user: null,
  logout: () => {},
  invalidateUserCache: async () => {},
});
UserContext.displayName = "UserContext";

export default UserContext;
