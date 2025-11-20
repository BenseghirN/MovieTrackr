import { Injectable, inject } from '@angular/core';
import {
	HttpClient,
	HttpErrorResponse,
	HttpHeaders,
	HttpParams,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

type Primitive = string | number | boolean;

export interface ApiRequestOptions {
	params?: Record<string, Primitive> | HttpParams;
	headers?: Record<string, string> | HttpHeaders;
	withCredentials?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
	private http = inject(HttpClient);
	private authToken: string | null = null;

	setAuthToken(token: string | null) {
		this.authToken = token;
	}

	get<T>(url: string, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.get<T>(url, this.buildOptions(options))
			.pipe(catchError((err) => this.handleError(err)));
	}

	post<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.post<T>(url, body, this.buildOptions(options))
			.pipe(catchError((err) => this.handleError(err)));
	}

	put<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.put<T>(url, body, this.buildOptions(options))
			.pipe(catchError((err) => this.handleError(err)));
	}

	patch<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.patch<T>(url, body, this.buildOptions(options))
			.pipe(catchError((err) => this.handleError(err)));
	}

	delete<T>(url: string, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.delete<T>(url, this.buildOptions(options))
			.pipe(catchError((err) => this.handleError(err)));
	}

	private buildOptions(options?: ApiRequestOptions) {
		const httpOptions: {
			params?: HttpParams;
			headers?: HttpHeaders;
			withCredentials?: boolean;
		} = {
            withCredentials: true
        };

		if (options?.params) {
			httpOptions.params = options.params instanceof HttpParams ? options.params : this.toHttpParams(options.params);
		}

         if (typeof options?.withCredentials === 'boolean') {
            httpOptions.withCredentials = options.withCredentials;
        }

		const headers = options?.headers instanceof HttpHeaders ? options.headers : new HttpHeaders(options?.headers ?? {});
		const finalHeaders = this.authToken ? headers.set('Authorization', `Bearer ${this.authToken}`) : headers;
		httpOptions.headers = finalHeaders;

		if (typeof options?.withCredentials === 'boolean') {
			httpOptions.withCredentials = options!.withCredentials;
		}

		return httpOptions;
	}

	private toHttpParams(params: Record<string, Primitive> | undefined) {
		let httpParams = new HttpParams();
		if (!params) return httpParams;
		for (const [k, v] of Object.entries(params)) {
			if (v === undefined || v === null) continue;
			httpParams = httpParams.set(k, String(v));
		}
		return httpParams;
	}

	private handleError(error: HttpErrorResponse): Observable<never> {
		const message = error.error && typeof error.error === 'string'
			? error.error
			: error.message || 'An unknown error occurred';
		return throwError(() => new Error(message));
	}
}
