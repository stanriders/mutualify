import { createContext } from 'react'

const UserContext = createContext({
  user: null,
})
UserContext.displayName = 'UserContext'

export default UserContext