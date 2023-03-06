import React, { useState, useCallback } from 'react';

interface IAuthProviderProps {
    children: React.ReactNode
}

interface IAuthContext {
    loggedIn: boolean,
    setLoggedIn: (data: boolean) => void
}

const defaultValue: IAuthContext = {
    loggedIn: false,
    setLoggedIn: loggedIn => { }
}

const AuthContext = React.createContext<IAuthContext>(defaultValue);
const useAuth = () => React.useContext(AuthContext)

export default function AuthProvider({ children }: IAuthProviderProps)
{
    const token = localStorage.getItem("token");

    const [loggedIn, setLoggedIn] = useState<boolean>(!!token);
    const contextValue: IAuthContext = { loggedIn, setLoggedIn };

    return (
        <AuthContext.Provider value={contextValue}>
            {children}
        </AuthContext.Provider>
    );
}

export { AuthProvider, useAuth }