import { Alert, Box, Button, Container, Divider, Grid, LinearProgress, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import React, { useCallback, useEffect, useState } from 'react';
import useApi from '../hooks/useApi';

interface ICustomization {
    border: boolean
}

const Customization = () => {

    const api = useApi<ICustomization>();
    const saveChangesApi = useApi<any>();

    const [message, setMessage] = useState<string | null>(null);
    const [border, setBorder] = useState<boolean>(true);

    useEffect(() => {
        api.send({ url: "/api/customization/get", callback: res => setBorder(res.data.border) });
    }, []);

    const onBorderChange = useCallback((event: any, newOption: boolean) => {
        if (newOption !== null)
            setBorder(newOption);
    }, [])

    const saveChanges = useCallback(() => {
        setMessage(null);
        saveChangesApi.send({
            url: "/api/customization/update",
            method: "post",
            data: {
                border: border
            },
            callback: res => setMessage("Changes saved successfully.")
        });
    }, [border]);

    return (
        <Container>
            <Grid textAlign="center">
                <img height={100} src="/htb.png"></img>
                <Typography variant="h4" fontWeight="bold" marginBottom={2}>HTB Updates</Typography>
                <Divider />
                <Typography variant="body1" marginTop={2} marginBottom={3}>Here you can customize your solve banner!</Typography>
                {message && (<Alert severity="success" sx={{marginBottom: 3}}>{message}</Alert>)}
                {api.response === null ? (
                    <LinearProgress />
                ) : (
                    <>
                        <ToggleButtonGroup exclusive value={border} onChange={onBorderChange} fullWidth>
                            <ToggleButton value={true}>
                                <Box>
                                    <Typography variant="body1" marginBottom={1}>With Border</Typography>
                                    <Box component="img" src="/border.png" width="100%" />
                                </Box>
                            </ToggleButton >
                            <ToggleButton value={false}>
                                <Box>
                                    <Typography variant="body1" marginBottom={1}>Without Border</Typography>
                                    <Box component="img" src="/borderless.png" width="100%" />
                                </Box>
                            </ToggleButton>
                        </ToggleButtonGroup >
                        <Button sx={{ marginTop: 3 }} onClick={saveChanges} variant="contained" size="large" endIcon={<i className="fa-solid fa-floppy-disk"></i>} disabled={saveChangesApi.loading}>
                            Save Changes
                        </Button>
                    </>

                )}
            </Grid>
        </Container>
    );
}

export default Customization;