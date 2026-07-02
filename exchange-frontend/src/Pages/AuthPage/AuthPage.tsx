import { useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import type { AuthContextValue } from '../../Auth/types';
import { useAuth } from '../../Auth/useAuth';
import styles from './AuthPage.module.css';

type Mode = 'login' | 'register';
type FormValues = { email: string; password: string; displayName: string };
type Field = keyof FormValues;

export default function AuthPage({ mode }: { mode: Mode }) {
  const auth = useAuth();
  const location = useLocation();
  const form = useAuthForm(mode);
  if (auth.status === 'authenticated') {
    return <Navigate to={returnPath(location.state)} replace />;
  }
  return <AuthLayout mode={mode} form={form} message={form.message ?? auth.message} />;
}

function useAuthForm(mode: Mode) {
  const auth = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [values, setValues] = useState<FormValues>(emptyForm);
  const [message, setMessage] = useState<string | null>(null);
  const submit = (event: FormEvent<HTMLFormElement>) => submitAuth(event, mode, values, auth, setMessage, () => navigate(returnPath(location.state), { replace: true }));
  return { values, setValues, submit, message, submitting: auth.status === 'loading' };
}

function AuthLayout({ mode, form, message }: LayoutProps) {
  return (
    <main className={styles.page}>
      <section className={styles.panel}>
        <AuthHeader mode={mode} />
        <AuthForm mode={mode} form={form} />
        {message && <p className={styles.message} role="alert">{message}</p>}
        <AuthSwitch mode={mode} />
      </section>
    </main>
  );
}

function AuthHeader({ mode }: { mode: Mode }) {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>{mode === 'login' ? 'Sign in' : 'Create account'}</p>
      <h1 className={styles.title}>{mode === 'login' ? 'Return to your Regulas workspace.' : 'Start a private Regulas workspace.'}</h1>
      <p className={styles.note}>Your portfolio, notes, and saved predictions stay tied to your account.</p>
    </header>
  );
}

function AuthForm({ mode, form }: AuthFormProps) {
  return (
    <form className={styles.form} onSubmit={form.submit}>
      {mode === 'register' && <AuthInput field="displayName" label="Display name" form={form} />}
      <AuthInput field="email" label="Email" form={form} type="email" />
      <AuthInput field="password" label="Password" form={form} type="password" />
      <button type="submit" disabled={form.submitting}>{mode === 'login' ? 'Sign in' : 'Create account'}</button>
    </form>
  );
}

function AuthInput({ field, label, form, type = 'text' }: InputProps) {
  return (
    <label className={styles.field}>
      <span>{label}</span>
      <input type={type} value={form.values[field]} onChange={inputChanged(form.setValues, field)} required />
    </label>
  );
}

function AuthSwitch({ mode }: { mode: Mode }) {
  const prompt = mode === 'login' ? 'Need an account?' : 'Already have an account?';
  const link = mode === 'login' ? '/register' : '/login';
  const label = mode === 'login' ? 'Create one' : 'Sign in';
  return <p className={styles.switch}>{prompt} <Link to={link}>{label}</Link></p>;
}

async function submitAuth(event: FormEvent<HTMLFormElement>, mode: Mode, values: FormValues, auth: AuthContextValue, setMessage: SetMessage, onSuccess: () => void) {
  event.preventDefault();
  setMessage(null);
  const result = await authRequest(mode, values, auth);
  if (result.ok) {
    onSuccess();
    return;
  }
  setMessage(result.message);
}

function authRequest(mode: Mode, values: FormValues, auth: AuthContextValue) {
  return mode === 'login' ? auth.login(loginRequest(values)) : auth.register(registerRequest(values));
}

function inputChanged(setValues: SetValues, field: Field) {
  return (event: ChangeEvent<HTMLInputElement>) => setValues((values) => ({ ...values, [field]: event.target.value }));
}

function loginRequest(values: FormValues) {
  return { email: values.email, password: values.password };
}

function registerRequest(values: FormValues) {
  return { ...loginRequest(values), displayName: values.displayName };
}

function returnPath(state: unknown) {
  return routeState(state)?.from?.pathname ?? '/portfolio';
}

function routeState(state: unknown) {
  return typeof state === 'object' && state !== null ? state as RouteState : null;
}

type RouteState = { from?: { pathname?: string } };
type SetMessage = (message: string | null) => void;
type SetValues = Dispatch<SetStateAction<FormValues>>;

type FormState = ReturnType<typeof useAuthForm>;

type LayoutProps = {
  mode: Mode;
  form: FormState;
  message: string | null;
};

type AuthFormProps = {
  mode: Mode;
  form: FormState;
};

type InputProps = {
  field: Field;
  label: string;
  form: FormState;
  type?: string;
};

const emptyForm = { email: '', password: '', displayName: '' } satisfies FormValues;
