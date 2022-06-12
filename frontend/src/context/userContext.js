import React from 'react'

const UserContext = React.createContext({
  user: null,
})
UserContext.displayName = 'UserContext'

export default UserContext