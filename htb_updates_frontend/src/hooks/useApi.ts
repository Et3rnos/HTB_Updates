import axios, { AxiosResponse } from "axios";
import { useState } from "react";
import { useAuth } from "../contexts/AuthProvider";

interface SendConfig {
    url: string,
    method?: string,
    data?: any,
    authenticated?: boolean,
    callback?: (res: AxiosResponse) => void
}

function useApi<T>() {
    const [response, setResponse] = useState<T | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const auth = useAuth();

    function send({ url, method = "get", data = {}, authenticated = true, callback = res => { } }: SendConfig) {
        setLoading(true);

        const headers = authenticated ? { Authorization: `Bearer ${localStorage.getItem("token")}` } : {}

        axios({
            url: url,
            method: method,
            headers: headers,
            data: data
        })
            .then(res => {
                setResponse(res.data);
                callback(res);
            })
            .catch(err => {
                if (err.response.status === 401) {
                    localStorage.removeItem("token");
                    auth.setLoggedIn(false);
                }
                else if (err.response.status === 500) {
                    
                }
                else if (err.response.data.error) {
                    const error = err.response.data.error;
                    setError(error);
                }
            })
            .finally(() => setLoading(false))
    }

    return { response, error, loading, send }
}

export default useApi;