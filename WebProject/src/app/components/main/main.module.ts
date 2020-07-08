// Modules
import { LOCALE_ID, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// Components
import { MainComponent } from './main.component';

// Services
import { MainService } from '../../services/main.service';
import { HttpClient } from '@angular/common/http';

@NgModule({
    imports: [
        CommonModule,
        RouterModule
    ],
    declarations: [
        MainComponent
    ],
    providers: [
        HttpClient,
        MainService,
        { provide: LOCALE_ID, useValue: 'en' }
    ]
})

export class MainModule { }
