# Frontend Guidelines — Dating App (Angular 19)

**Purpose:** Short, opinionated rules and examples to keep the frontend modern, maintainable and high-performance. Follow Angular team recommendations: prefer standalone components, Signals for local UI state, `OnPush` change detection, `inject()` for DI, and typed Reactive Forms.

---

## Key Principles

- Use **the newest safe features** (Angular 19): standalone components, Signals, `inject()`, typed forms
- Prefer **OnPush** change detection over `ngOnChanges` for performance and predictable renders
- Use **feature-based** structure (each feature folder holds components, services, store, tests)
- Keep components small — composition over huge components
- Use **NgRx** for global state that is shared/complex. Use **Signals** for local component state or derived UI state
- Use **Tailwind CSS** and PrimeNG components for fast, consistent UI

---

## 1. Component Architecture

### Standalone Components (Required)
All components must be standalone. No NgModules except for lazy-loaded feature modules.

```typescript
@Component({
  selector: 'matcha-user-card',
  standalone: true,
  imports: [CommonModule, Button, Avatar, Badge],
  templateUrl: './user-card.component.html',
  styleUrl: './user-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserCardComponent {
  // Component logic
}
```

### Component Structure
```
src/app/features/user/components/user-card/
├── user-card.component.ts
├── user-card.component.html
├── user-card.component.css
└── user-card.component.spec.ts
```

### Dependency Injection with `inject()`
Always use `inject()` function instead of constructor injection.

```typescript
export class UserCardComponent {
  private userService = inject(UserService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  
  // Component logic
}
```

---

## 2. State Management

### NgRx for Global State
Use NgRx for complex, shared state across features.

```typescript
// user.store.ts
export interface UserState {
  users: User[];
  selectedUser: User | null;
  loading: boolean;
  error: string | null;
}

export const userFeature = createFeature({
  name: 'user',
  reducer: createReducer(
    initialState,
    on(UserActions.loadUsers, (state) => ({ ...state, loading: true })),
    on(UserActions.loadUsersSuccess, (state, { users }) => ({ 
      ...state, 
      users, 
      loading: false 
    }))
  )
});
```

### Signals for Local State
Use Signals for component-local state and derived values.

```typescript
export class UserCardComponent {
  private userService = inject(UserService);
  
  // Local state with Signals
  private user = signal<User | null>(null);
  private isLoading = signal(false);
  
  // Derived state
  readonly displayName = computed(() => {
    const userData = this.user();
    return userData ? `${userData.firstName} ${userData.lastName}` : 'Unknown User';
  });
  
  readonly isOnline = computed(() => {
    const userData = this.user();
    return userData?.lastSeen ? 
      new Date(userData.lastSeen) > new Date(Date.now() - 5 * 60 * 1000) : false;
  });
}
```

---

## 3. Forms & Validation

### Typed Reactive Forms
Always use typed reactive forms with proper validation.

```typescript
interface LoginForm {
  username: FormControl<string>;
  password: FormControl<string>;
  rememberMe: FormControl<boolean>;
}

export class LoginComponent {
  private fb = inject(FormBuilder);
  
  loginForm = this.fb.group<LoginForm>({
    username: this.fb.control('', [Validators.required, Validators.minLength(3)]),
    password: this.fb.control('', [Validators.required, Validators.minLength(6)]),
    rememberMe: this.fb.control(false)
  });
  
  onSubmit(): void {
    if (this.loginForm.valid) {
      const formValue = this.loginForm.getRawValue();
      // Type-safe form data
      this.authService.login(formValue);
    }
  }
}
```

### Custom Validators
Create reusable validators for common patterns.

```typescript
export function ageValidator(minAge: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const birthDate = new Date(control.value);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    
    return age >= minAge ? null : { minAge: { requiredAge: minAge, actualAge: age } };
  };
}
```

---

## 4. Performance Optimization

### OnPush Change Detection
Always use `OnPush` change detection strategy.

```typescript
@Component({
  selector: 'matcha-user-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  // ...
})
export class UserListComponent {
  // Component logic
}
```

### TrackBy Functions
Implement trackBy functions for ngFor loops.

```typescript
export class UserListComponent {
  trackByUserId(index: number, user: User): string {
    return user.id;
  }
}
```

```html
<div *ngFor="let user of users; trackBy: trackByUserId" class="user-item">
  <!-- User content -->
</div>
```

### Lazy Loading
Use lazy loading for feature modules.

```typescript
export const routes: Routes = [
  {
    path: 'users',
    loadComponent: () => import('./features/user/components/users-list/users-list.component')
      .then(m => m.UsersListComponent)
  },
  {
    path: 'chat',
    loadChildren: () => import('./features/chat/chat.routes')
  }
];
```

---

## 5. Styling Guidelines

### Tailwind CSS Classes
Use Tailwind utility classes for consistent styling.

```html
<div class="flex items-center justify-between p-4 bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow">
  <div class="flex items-center space-x-3">
    <img class="w-12 h-12 rounded-full object-cover" [src]="user.avatar" [alt]="user.name">
    <div>
      <h3 class="text-lg font-semibold text-gray-900">{{ user.name }}</h3>
      <p class="text-sm text-gray-500">{{ user.age }} • {{ user.location }}</p>
    </div>
  </div>
  <button class="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors">
    Like
  </button>
</div>
```

### PrimeNG Integration
Use PrimeNG components with Tailwind for consistent UI.

```typescript
@Component({
  imports: [
    Button,
    Card,
    Avatar,
    Badge,
    Dialog
  ],
  // ...
})
export class UserCardComponent {
  // Component logic
}
```

---

## 6. Error Handling

### Global Error Interceptor
Handle HTTP errors globally with user-friendly messages.

```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private messageService = inject(MessageService);
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        const errorMessage = this.getErrorMessage(error);
        
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage
        });
        
        return throwError(() => error);
      })
    );
  }
  
  private getErrorMessage(error: HttpErrorResponse): string {
    switch (error.status) {
      case 401: return 'Please log in to continue';
      case 403: return 'You do not have permission to perform this action';
      case 404: return 'The requested resource was not found';
      case 500: return 'A server error occurred. Please try again later';
      default: return 'An unexpected error occurred';
    }
  }
}
```

### Component Error Boundaries
Handle component-level errors gracefully.

```typescript
export class UserCardComponent {
  private errorHandler = inject(ErrorHandler);
  
  onUserAction(): void {
    try {
      // Risky operation
      this.performUserAction();
    } catch (error) {
      this.errorHandler.handleError(error);
    }
  }
}
```

---

## 7. Testing Guidelines

### Component Testing
Write focused unit tests for components.

```typescript
describe('UserCardComponent', () => {
  let component: UserCardComponent;
  let fixture: ComponentFixture<UserCardComponent>;
  
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserCardComponent],
      providers: [
        { provide: UserService, useValue: mockUserService }
      ]
    }).compileComponents();
    
    fixture = TestBed.createComponent(UserCardComponent);
    component = fixture.componentInstance;
  });
  
  it('should display user name correctly', () => {
    component.user = mockUser;
    fixture.detectChanges();
    
    const nameElement = fixture.debugElement.query(By.css('.user-name'));
    expect(nameElement.nativeElement.textContent).toBe(mockUser.name);
  });
});
```

### Service Testing
Test services with proper mocking.

```typescript
describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService]
    });
    
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });
  
  it('should fetch users', () => {
    const mockUsers: User[] = [mockUser];
    
    service.getUsers().subscribe(users => {
      expect(users).toEqual(mockUsers);
    });
    
    const req = httpMock.expectOne('/api/users');
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);
  });
});
```

---

## 8. File Naming Conventions

### Components
- `kebab-case.component.ts` for component files
- `PascalCase` for component class names
- `kebab-case` for selectors

### Services
- `kebab-case.service.ts` for service files
- `PascalCase` for service class names

### Models/Interfaces
- `kebab-case.model.ts` for model files
- `PascalCase` for interface names

### Examples
```
user-card.component.ts → UserCardComponent
user.service.ts → UserService
user.model.ts → User interface
```

---

## 9. Code Organization

### Feature Structure
```
src/app/features/user/
├── components/
│   ├── user-card/
│   ├── user-list/
│   └── user-profile/
├── services/
│   └── user.service.ts
├── store/
│   ├── user.actions.ts
│   ├── user.effects.ts
│   ├── user.reducer.ts
│   └── user.selectors.ts
├── models/
│   └── user.model.ts
└── user.routes.ts
```

### Shared Components
```
src/app/shared/
├── components/
│   ├── loading-spinner/
│   ├── error-message/
│   └── confirm-dialog/
├── directives/
├── pipes/
└── utils/
```

---

## 10. Best Practices Checklist

### Component Development
- [ ] Use standalone components
- [ ] Implement OnPush change detection
- [ ] Use `inject()` for dependency injection
- [ ] Keep components under 200 lines
- [ ] Use typed reactive forms
- [ ] Implement proper error handling

### State Management
- [ ] Use NgRx for complex global state
- [ ] Use Signals for local component state
- [ ] Avoid prop drilling with proper state management
- [ ] Implement proper loading and error states

### Performance
- [ ] Use trackBy functions for ngFor
- [ ] Implement lazy loading for routes
- [ ] Optimize images and assets
- [ ] Use OnPush change detection
- [ ] Implement proper memoization

### Testing
- [ ] Write unit tests for components
- [ ] Test services with proper mocking
- [ ] Implement integration tests for critical flows
- [ ] Maintain test coverage above 80%

### Code Quality
- [ ] Follow consistent naming conventions
- [ ] Use TypeScript strict mode
- [ ] Implement proper error boundaries
- [ ] Use ESLint and Prettier
- [ ] Write self-documenting code
