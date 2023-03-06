import { Box, Button, Container, Divider, Fab, Grid, Icon, Link, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import React, { useCallback, useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import LoginIcon from '@mui/icons-material/Login';

const Home = () => {

    const [state, setState] = useState<string>((Math.random() * 100000000000000000).toString())

    useEffect(() => {
        localStorage.setItem("state", state);
    }, []);

    return (
        <Container>
            <Grid textAlign="center">
                <Button
                    variant="contained"
                    color="primary"
                    sx={{ position: "fixed", top: 0, right: 0, borderRadius: 0, borderBottomLeftRadius: 20, paddingX: 6, paddingY: 2 }}
                    component={RouterLink}
                    to={"https://discord.com/api/oauth2/authorize?client_id=806824180074938419&redirect_uri=https%3A%2F%2Fhtbupdates.com%2Fcallback&response_type=code&scope=identify&state=" + state}
                >
                    <LoginIcon sx={{marginRight: 1}} /> Login
                </Button>
                <img height={100} src="/htb.png"></img>
                <Typography variant="h4" fontWeight="bold" marginBottom={2}>HTB Updates</Typography>
                <Divider />
                <Typography variant="body2" marginY={2}>HTB Updates is a discord bot that announces your members HackTheBox solves.</Typography>
                <Grid container>
                    <Grid item md={6} xs={12} padding={3 }>
                        <Typography variant="h5" fontWeight="bold" marginBottom={2}>Solve Announcements</Typography>
                        <Box component="img" src="/solve_announcement.png" width="100%" sx={{ borderRadius: 2 }} />
                    </Grid>
                    <Grid item md={6} xs={12} padding={3}>
                        <Typography variant="h5" fontWeight="bold" marginBottom={2}>Release Announcements</Typography>
                        <Box component="img" src="/machine_announcement.png" width="100%" sx={{borderRadius: 2 }} />
                    </Grid>
                </Grid>
                <Divider />
                <Typography variant="h5" fontWeight="bold" marginY={2}>How to get started?</Typography>
                <Divider />
                <Typography variant="body2" marginTop={2} marginBottom={3}>Getting started is very easy. Just follow these steps:</Typography>
                <Grid container textAlign="left">
                    <Grid item md={6} xs={12} padding={3}>
                        <Typography variant="h6" fontWeight="bold" marginBottom={2}>1. Invite the bot to your server</Typography>
                        <Divider />
                        <Typography variant="body2" marginTop={2} marginBottom={4}><Link color="inherit" sx={{ textUnderlineOffset: 5 }}>Click here</Link> to invite the bot to your server.</Typography>
                        <Typography variant="h6" fontWeight="bold" marginBottom={2}>2. Setup the bot</Typography>
                        <Divider />
                        <Typography variant="body2" marginTop={2} marginBottom={4}>Create the channel you would like the bot to send announcements to and then send the message h.setup in it.</Typography>
                        <Typography variant="h6" fontWeight="bold" marginBottom={2}>3. Link HTB accounts</Typography>
                        <Divider />
                        <Typography variant="body2" marginTop={2} marginBottom={4}>Ask your server members to link their HTB accounts using the h.link command.</Typography>
                    </Grid>
                    <Grid item md={6} xs={12} padding={3}>
                        <Box component="img" src="/setup.png" width="100%" sx={{ borderRadius: 2 }} />
                        <Box component="img" src="/link.png" width="100%" sx={{ borderRadius: 2 }} />
                    </Grid>
                </Grid>
                <Divider />
                <Typography variant="h5" fontWeight="bold" marginY={2}>About</Typography>
                <Divider />
                <Grid container>
                    <Grid item md={6} xs={12} padding={3}>
                        <Typography variant="h5" fontWeight="bold">Open Source</Typography>
                        <Typography variant="body2" marginTop={2} marginBottom={2}>Did you know? This bot is open source.</Typography>
                        <Button variant="outlined" color="info" startIcon={<i className="fab fa-github"></i>}>View On Github</Button>
                    </Grid>
                    <Grid item md={6} xs={12} padding={3}>
                        <Typography variant="h5" fontWeight="bold">Creator</Typography>
                        <Typography variant="body2" marginTop={2} marginBottom={2}>HTB Updates was created by Et3rnos.</Typography>
                        <Button variant="outlined" color="info" startIcon={<i className="fab fa-patreon"></i>}>Support me on Patreon</Button>
                    </Grid>
                </Grid>
            </Grid>
            
        </Container>
    );
}

export default Home;