import { ensurePurchaseTable } from '../../utils/schema'
import { prisma } from '../../utils/db'
import { requirePermission } from '../../utils/auth'

export default defineEventHandler(async (event) => {
  await requirePermission(event, 'purchases')
  await ensurePurchaseTable()
  return prisma.purchase.findMany({ orderBy: { createdAt: 'desc' } })
})
