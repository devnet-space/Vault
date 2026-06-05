package com.devnet.vault.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import com.devnet.vault.presentation.home.HomeScreen
import com.devnet.vault.presentation.splash.SplashScreen

@Composable
fun NavGraph() {

    var showSplash by remember { mutableStateOf(true) }

    if (showSplash) {
        SplashScreen(
            onNavigateToHome = {
                showSplash = false
            }
        )
    } else {
        HomeScreen()
    }
}