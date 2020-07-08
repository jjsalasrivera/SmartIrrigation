import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

const APP_ROUTES: Routes = [
  {
    path: 'main',
    loadChildren: () => import('./components/main/main.module').then(m => m.MainModule)
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(
            APP_ROUTES,
            {useHash: true, onSameUrlNavigation: 'ignore'}
    )
  ],
  exports: [RouterModule],
  providers: []
})
export class AppRoutingModule { }
