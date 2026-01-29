<template>
  <div class="space-y-4">
    <div class="admin-box flex items-center justify-between gap-2 flex-wrap">
      <div>
        <div class="text-lg font-black rtl-text">{{ t('admin.users.title') }}</div>
        <div class="text-sm admin-muted rtl-text">{{ t('admin.users.hint') }}</div>
      </div>

      <div class="flex items-center gap-2 flex-wrap">
        <input
          v-model="q"
          class="admin-input w-[240px]"
          :placeholder="t('admin.users.search')"
          @keydown.enter="load(1)"
        />

        <select v-model="role" class="admin-input w-[170px]">
          <option value="">{{ t('admin.users.allRoles') }}</option>
          <option value="Admin">Admin</option>
          <option value="User">User</option>
        </select>

        <UiButton variant="secondary" @click="load(1)">
          <Icon name="mdi:magnify" class="text-lg" />
          <span class="rtl-text">{{ t('admin.users.filter') }}</span>
        </UiButton>

        <UiButton @click="openCreate = true">
          <Icon name="mdi:account-plus-outline" class="text-lg" />
          <span class="rtl-text">{{ t('admin.users.create') }}</span>
        </UiButton>
      </div>
    </div>

    <div class="admin-box">
      <div v-if="loading" class="admin-muted rtl-text">{{ t('loading') }}...</div>
      <div v-else-if="error" class="text-red-500 rtl-text">{{ error }}</div>

      <!-- Desktop table -->
      <div class="hidden md:block overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="text-left border-b border-app">
              <th class="py-3 px-2">{{ t('admin.users.name') }}</th>
              <th class="py-3 px-2">{{ t('admin.users.email') }}</th>
              <th class="py-3 px-2">{{ t('admin.users.role') }}</th>
              <th class="py-3 px-2">{{ t('admin.users.status') }}</th>
              <th class="py-3 px-2 text-right">{{ t('admin.users.actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in items" :key="u.id" class="border-b border-app/70">
              <td class="py-3 px-2 font-bold">{{ u.name || '—' }}</td>
              <td class="py-3 px-2 keep-ltr">{{ u.email }}</td>
              <td class="py-3 px-2">
                <span class="badge" :class="u.role === 'Admin' ? 'badge-admin' : ''">{{ u.role }}</span>
              </td>
              <td class="py-3 px-2">
                <span class="badge" :class="u.isActive ? 'badge-on' : 'badge-off'">
                  {{ u.isActive ? t('admin.users.active') : t('admin.users.blocked') }}
                </span>
              </td>
              <td class="py-3 px-2 text-right">
                <div class="flex items-center justify-end gap-2">
                  <UiButton variant="ghost" class="px-3" @click="edit(u)">
                    <Icon name="mdi:pencil-outline" class="text-lg" />
                    <span class="rtl-text">{{ t('edit') }}</span>
                  </UiButton>
                  <UiButton variant="ghost" class="px-3" @click="confirmDelete(u)">
                    <Icon name="mdi:trash-can-outline" class="text-lg" />
                    <span class="rtl-text">{{ t('delete') }}</span>
                  </UiButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile cards -->
      <div class="md:hidden grid gap-3">
        <div v-for="u in items" :key="u.id" class="rounded-2xl border border-app bg-surface p-4">
          <div class="flex items-start justify-between gap-3">
            <div class="min-w-0">
              <div class="font-black rtl-text truncate">{{ u.name || '—' }}</div>
              <div class="text-sm admin-muted keep-ltr truncate">{{ u.email }}</div>
            </div>
            <div class="flex gap-2">
              <button class="icon-btn" @click="edit(u)" :title="t('edit')">
                <Icon name="mdi:pencil-outline" class="text-xl" />
              </button>
              <button class="icon-btn" @click="confirmDelete(u)" :title="t('delete')">
                <Icon name="mdi:trash-can-outline" class="text-xl" />
              </button>
            </div>
          </div>

          <div class="mt-3 flex items-center gap-2 flex-wrap">
            <span class="badge" :class="u.role === 'Admin' ? 'badge-admin' : ''">{{ u.role }}</span>
            <span class="badge" :class="u.isActive ? 'badge-on' : 'badge-off'">
              {{ u.isActive ? t('admin.users.active') : t('admin.users.blocked') }}
            </span>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div class="mt-4 flex items-center justify-between gap-3 flex-wrap">
        <div class="text-sm admin-muted keep-ltr">{{ page }} / {{ totalPages }}</div>
        <div class="flex gap-2">
          <UiButton variant="secondary" :disabled="page <= 1" @click="load(page - 1)">
            <Icon name="mdi:chevron-right" class="text-xl" />
            <span class="rtl-text">{{ t('prev') }}</span>
          </UiButton>
          <UiButton variant="secondary" :disabled="page >= totalPages" @click="load(page + 1)">
            <span class="rtl-text">{{ t('next') }}</span>
            <Icon name="mdi:chevron-left" class="text-xl" />
          </UiButton>
        </div>
      </div>
    </div>

    <!-- Edit modal -->
    <Teleport to="body">
      <div v-if="openEdit" class="fixed inset-0 z-[120]">
        <div class="absolute inset-0 bg-black/50" @click="closeAll" />
        <div class="absolute inset-x-3 top-20 mx-auto max-w-xl rounded-3xl border border-app bg-app p-4">
          <div class="flex items-center justify-between gap-2">
            <div class="font-black rtl-text">{{ t('admin.users.editTitle') }}</div>
            <button class="icon-btn" @click="closeAll"><Icon name="mdi:close" class="text-xl" /></button>
          </div>

          <div class="mt-4 grid gap-3">
            <div>
              <div class="text-xs admin-muted rtl-text">{{ t('admin.users.name') }}</div>
              <input v-model="form.name" class="admin-input" />
            </div>
            <div>
              <div class="text-xs admin-muted rtl-text">{{ t('admin.users.email') }}</div>
              <input v-model="form.email" class="admin-input keep-ltr" />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <div class="text-xs admin-muted rtl-text">{{ t('admin.users.role') }}</div>
                <select v-model="form.role" class="admin-input">
                  <option value="Admin">Admin</option>
                  <option value="User">User</option>
                </select>
              </div>
              <div>
                <div class="text-xs admin-muted rtl-text">{{ t('admin.users.status') }}</div>
                <select v-model="form.isActive" class="admin-input">
                  <option :value="true">{{ t('admin.users.active') }}</option>
                  <option :value="false">{{ t('admin.users.blocked') }}</option>
                </select>
              </div>
            </div>
            <div>
              <div class="text-xs admin-muted rtl-text">{{ t('admin.users.newPassword') }}</div>
              <input v-model="form.newPassword" type="password" class="admin-input keep-ltr" :placeholder="t('admin.users.optional')" />
            </div>
          </div>

          <div class="mt-4 flex items-center justify-end gap-2">
            <UiButton variant="secondary" @click="closeAll">
              <span class="rtl-text">{{ t('cancel') }}</span>
            </UiButton>
            <UiButton :disabled="saving" @click="save">
              <Icon name="mdi:content-save-outline" class="text-lg" />
              <span class="rtl-text">{{ t('save') }}</span>
            </UiButton>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Create modal -->
    <Teleport to="body">
      <div v-if="openCreate" class="fixed inset-0 z-[120]">
        <div class="absolute inset-0 bg-black/50" @click="closeAll" />
        <div class="absolute inset-x-3 top-20 mx-auto max-w-xl rounded-3xl border border-app bg-app p-4">
          <div class="flex items-center justify-between gap-2">
            <div class="font-black rtl-text">{{ t('admin.users.createTitle') }}</div>
            <button class="icon-btn" @click="closeAll"><Icon name="mdi:close" class="text-xl" /></button>
          </div>

          <div class="mt-4 grid gap-3">
            <div>
              <div class="text-xs admin-muted rtl-text">{{ t('admin.users.name') }}</div>
              <input v-model="createForm.name" class="admin-input" />
            </div>
            <div>
              <div class="text-xs admin-muted rtl-text">{{ t('admin.users.email') }}</div>
              <input v-model="createForm.email" class="admin-input keep-ltr" />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <div class="text-xs admin-muted rtl-text">{{ t('admin.users.role') }}</div>
                <select v-model="createForm.role" class="admin-input">
                  <option value="Admin">Admin</option>
                  <option value="User">User</option>
                </select>
              </div>
              <div>
                <div class="text-xs admin-muted rtl-text">{{ t('admin.users.password') }}</div>
                <input v-model="createForm.password" type="password" class="admin-input keep-ltr" />
              </div>
            </div>
          </div>

          <div class="mt-4 flex items-center justify-end gap-2">
            <UiButton variant="secondary" @click="closeAll">
              <span class="rtl-text">{{ t('cancel') }}</span>
            </UiButton>
            <UiButton :disabled="saving" @click="create">
              <Icon name="mdi:account-plus-outline" class="text-lg" />
              <span class="rtl-text">{{ t('create') }}</span>
            </UiButton>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete confirm -->
    <Teleport to="body">
      <div v-if="openDelete" class="fixed inset-0 z-[120]">
        <div class="absolute inset-0 bg-black/50" @click="closeAll" />
        <div class="absolute inset-x-3 top-28 mx-auto max-w-lg rounded-3xl border border-app bg-app p-4">
          <div class="font-black rtl-text">{{ t('admin.users.deleteTitle') }}</div>
          <div class="mt-2 text-sm admin-muted rtl-text">
            {{ t('admin.users.deleteHint') }}
            <span class="keep-ltr font-bold">{{ pendingDelete?.email }}</span>
          </div>
          <div class="mt-4 flex justify-end gap-2">
            <UiButton variant="secondary" @click="closeAll">{{ t('cancel') }}</UiButton>
            <UiButton :disabled="saving" @click="remove">
              <Icon name="mdi:trash-can-outline" class="text-lg" />
              <span class="rtl-text">{{ t('delete') }}</span>
            </UiButton>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'

type UserRow = {
  id: string
  email: string
  name: string | null
  role: string
  isActive: boolean
  createdAt: string
}

const { t } = useI18n()
const api = useApi()

const q = ref('')
const role = ref<string>('')

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)

const items = ref<UserRow[]>([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

const openEdit = ref(false)
const openCreate = ref(false)
const openDelete = ref(false)

const pendingDelete = ref<UserRow | null>(null)

const form = reactive({
  id: '',
  name: '',
  email: '',
  role: 'User',
  isActive: true,
  newPassword: ''
})

const createForm = reactive({
  name: '',
  email: '',
  role: 'User',
  password: ''
})

async function load(p = 1) {
  page.value = p
  loading.value = true
  error.value = null
  try {
    const res = await api.get<any>('/admin/users', {
      params: {
        page: page.value,
        pageSize: pageSize.value,
        q: q.value || undefined,
        role: role.value || undefined
      }
    })
    items.value = res?.items || []
    total.value = res?.total || 0
  } catch (e: any) {
    error.value = e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

function edit(u: UserRow) {
  form.id = u.id
  form.name = u.name || ''
  form.email = u.email || ''
  form.role = u.role || 'User'
  form.isActive = !!u.isActive
  form.newPassword = ''
  openEdit.value = true
}

function confirmDelete(u: UserRow) {
  pendingDelete.value = u
  openDelete.value = true
}

function closeAll() {
  openEdit.value = false
  openCreate.value = false
  openDelete.value = false
  pendingDelete.value = null
}

async function save() {
  if (!form.id) return
  saving.value = true
  try {
    await api.put(`/admin/users/${form.id}`, {
      name: form.name,
      email: form.email,
      role: form.role,
      isActive: form.isActive,
      newPassword: form.newPassword || undefined
    })
    closeAll()
    await load(page.value)
  } finally {
    saving.value = false
  }
}

async function create() {
  saving.value = true
  try {
    await api.post(`/admin/users`, {
      name: createForm.name,
      email: createForm.email,
      role: createForm.role,
      password: createForm.password
    })
    createForm.name = ''
    createForm.email = ''
    createForm.role = 'User'
    createForm.password = ''
    closeAll()
    await load(1)
  } finally {
    saving.value = false
  }
}

async function remove() {
  if (!pendingDelete.value) return
  saving.value = true
  try {
    await api.del(`/admin/users/${pendingDelete.value.id}`)
    closeAll()
    await load(Math.min(page.value, totalPages.value))
  } finally {
    saving.value = false
  }
}

onMounted(() => load(1))
</script>

<style scoped>
.badge{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--muted));
  font-weight: 900;
}
.badge-on{ border-color: rgba(16,185,129,.35); background: rgba(16,185,129,.12); color: rgb(var(--fg)); }
.badge-off{ border-color: rgba(239,68,68,.35); background: rgba(239,68,68,.12); color: rgb(var(--fg)); }
.badge-admin{ border-color: rgba(124,58,237,.35); background: rgba(124,58,237,.12); color: rgb(var(--fg)); }

.admin-box{ border-radius: 20px; border: 1px solid rgb(var(--border)); background: rgb(var(--surface)); padding: 16px; }
.admin-muted{ color: rgb(var(--muted)); }
.admin-input{ width: 100%; border-radius: 14px; border: 1px solid rgb(var(--border)); background: rgb(var(--surface-2)); padding: 10px 12px; outline: none; }
.icon-btn{ height: 40px; width: 40px; display: grid; place-items: center; border-radius: 16px; border: 1px solid rgb(var(--border)); background: rgb(var(--surface-2)); }
</style>
