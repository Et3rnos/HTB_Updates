import { Box, Button, CircularProgress, Container, Grid, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import React, { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthProvider';
import useApi from '../hooks/useApi';

const Callback = () => {

    const [searchParams, setSearchParams] = useSearchParams();

    const auth = useAuth();
    const navigate = useNavigate();

    const api = useApi<any>();

    useEffect(() => {
        if (searchParams.get("state") !== localStorage.getItem("state"))
            navigate("/");
        else {
            localStorage.removeItem("state");
            api.send({
                url: "/api/auth/login",
                method: "post",
                data: {
                    code: searchParams.get("code")
                },
                callback: res => {
                    auth.setLoggedIn(true);
                    localStorage.setItem("token", res.data.token);
                    navigate("/customization");
                }
            })
        }
    }, []);

    return (
        <Grid container spacing={0} direction="column" alignItems="center" justifyContent="center" style={{ minHeight: '100vh' }}>
            <CircularProgress size="5rem" />
        </Grid>
    );
}

export default Callback;