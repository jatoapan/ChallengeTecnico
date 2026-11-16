import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificacionService } from '../../core/services/notificacion.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private notificaciones = inject(NotificacionService);

  cargando = signal(false);
  verClave = signal(false);

  formulario = this.fb.nonNullable.group({
    usuario: ['', [Validators.required, Validators.maxLength(20)]],
    password: ['', [Validators.required]]
  });

  ingresar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.cargando.set(true);

    this.auth.login(this.formulario.getRawValue()).subscribe({
      next: respuesta => {
        this.notificaciones.exito(`Bienvenido, ${respuesta.usuario}.`);
        this.router.navigate(['/']);
      },
      error: () => this.cargando.set(false)
    });
  }
}