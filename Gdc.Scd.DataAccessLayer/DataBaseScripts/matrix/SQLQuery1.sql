SELECT N AS number, L AS letter, M  FROM
    (VALUES (1), (2), (3)) a(N)
CROSS JOIN
    (VALUES ('A'), ('B'), ('C')) b(L)
CROSS JOIN
    (VALUES ('wg1'), ('wg2'), ('wg3')) c(M)

order by N
